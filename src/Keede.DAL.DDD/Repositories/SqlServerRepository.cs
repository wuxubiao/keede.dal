using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Extension;
using Keede.DAL.RWSplitting;
using System.Data.SqlClient;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class SqlServerRepository<TEntity> : RepositoryWithTransaction<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public IDbConnection OpenDbConnection(bool isReadDb = true)
        {
            var conn = DbTransaction != null ? DbTransaction.Connection : Databases.GetDbConnection(isReadDb);
            if (conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed) conn.Open();
            return conn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        public void CloseConnection(IDbConnection conn)
        {
            if (DbTransaction != null) return;
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Add(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = conn.Insert(data, DbTransaction) > 0;
            CloseConnection(conn);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="destinationTableName"></param>
        /// <returns></returns>
        public override bool BatchAdd<T>(IList<T> list, string destinationTableName = null)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            TypeMapper.SetTypeMap(typeof(T));
            var conn = OpenDbConnection(false);
            var dt = conn.GetTableSchema(list);
            if (!string.IsNullOrEmpty(destinationTableName)) dt.TableName = destinationTableName;
            var value = BulkToDb(conn, dt, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        private static bool BulkToDb(IDbConnection conn, DataTable dt, IDbTransaction dbTransaction)
        {
            SqlConnection sqlConn = conn as SqlConnection;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction);
            bulkCopy.DestinationTableName = dt.TableName;
            bulkCopy.BatchSize = dt.Rows.Count;

            try
            {
                if (dt.Rows.Count != 0)
                    bulkCopy.WriteToServer(dt);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                bulkCopy.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Save(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = conn.Update(data, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Remove(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = conn.Delete(data, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override int Remove(string whereSql, object parameterObject = null)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = conn.Delete<TEntity>(whereSql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override TEntity Get(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var value = conn.QueryFirstOrDefault<TEntity>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override T Get<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            TypeMapper.SetTypeMap(typeof(T));
            var conn = OpenDbConnection(isReadDb);
            var value = conn.QueryFirstOrDefault<T>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override TEntity Get(object condition, bool isReadDb = true)
        {
            return GetList(condition, isReadDb).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override TEntity GetById(dynamic id, bool isReadDb = true)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var value = SqlMapperExtensions.Get<TEntity>(conn, id, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// isUpdateLock使用WITH (UPDLOCK)，其他事务可读取，不可更新
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public override TEntity GetAndUpdateLock(object condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            return conn.GetAndUpdateLock<TEntity>(condition, table, "*", false, DbTransaction, 3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(T));
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var values = conn.Query<T>(sql, parameterObject, DbTransaction).ToList();
            CloseConnection(conn);
            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override IList<TEntity> GetList(object condition, bool isReadDb = true)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            return conn.QueryList<TEntity>(condition, table, "*", false, DbTransaction, 3).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IList<TEntity> GetAll(bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var list = conn.GetAll<TEntity>(DbTransaction).ToList();
            CloseConnection(conn);
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override int GetCount(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var values = (int)conn.ExecuteScalar(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override PagedList<TEntity> GetPagedList(string whereSql, string orderBy, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            PagedList<TEntity> pagedList = new PagedList<TEntity>(pageIndex, pageSize, whereSql, orderBy);
            conn.QueryPaged(ref pagedList, parameterObjects, DbTransaction);
            CloseConnection(conn);
            return pagedList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">排序的字段必须select出来且不能带别名，如ORDER by XX.CreateTime将会报错，ORDER by CreateTime正常
        /// sql语句须确保没有多余的空格</param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override IList<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(T));
            var conn = OpenDbConnection(isReadDb);
            var pagedList = conn.QueryPaged<T>(sql, pageIndex, pageSize, orderBy, parameterObjects, DbTransaction);
            CloseConnection(conn);
            return pagedList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override PagedList<TEntity> GetPagedList(object condition, string orderBy, int pageIndex, int pageSize, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            return conn.QueryPaged<TEntity>(condition, table, orderBy, pageIndex, pageSize, "*", false, DbTransaction, 3);
        }
    }
}
