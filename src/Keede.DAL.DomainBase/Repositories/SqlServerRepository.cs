using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Extension;
using Keede.DAL.RWSplitting;
using System.Data.SqlClient;

namespace Keede.DAL.DomainBase.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SqlServerRepository<TEntity> : RepositoryWithTransaction<TEntity>
        where TEntity : class, IEntity
    {
        private IDbConnection OpenDbConnection(bool isReadDb = true)
        {
            var conn = DbTransaction != null ? DbTransaction.Connection : Databases.GetDbConnection(isReadDb);
            if (conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed) conn.Open();
            return conn;
        }

        private void CloseConnection(IDbConnection conn)
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
            var conn = OpenDbConnection(false);
            var value = conn.InsertEx(data, DbTransaction) > 0;
            CloseConnection(conn);
            return value;
        }

        public override bool BatchAdd<T>(IList<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            var conn = OpenDbConnection(false);
            var dt = conn.GetTableSchema(list);
            var value = BulkToDB(conn, dt);
            //var value = conn.InsertEx(list, DbTransaction) > 0;
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static bool BulkToDB(IDbConnection conn, DataTable dt)
        {
            SqlConnection sqlConn = conn as SqlConnection;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn);
            bulkCopy.DestinationTableName = dt.TableName;
            bulkCopy.BatchSize = dt.Rows.Count;

            try
            {
                if (dt != null && dt.Rows.Count != 0)
                    bulkCopy.WriteToServer(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (bulkCopy != null)
                    bulkCopy.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Save(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
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
            var conn = OpenDbConnection(false);
            var value = conn.Delete(data, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override int Remove(string whereSql, object parameterObject = null)
        {
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
        /// <returns></returns>
        public override TEntity Get(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var value = conn.QueryFirstOrDefault<TEntity>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }


        public override T Get<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var value = conn.QueryFirstOrDefault<T>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override TEntity GetById(dynamic id, bool isReadDb = true)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var conn = OpenDbConnection(isReadDb);
            var value = SqlMapperExtensions.Get<TEntity>(conn, id, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isUpdateLock"></param>
        /// <returns></returns>
        [Obsolete("isUpdateLock未实现")]
        public override TEntity GetById(dynamic id, bool isUpdateLock, bool isReadDb = true)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var conn = OpenDbConnection(isReadDb);
            var value = SqlMapperExtensions.Get<TEntity>(conn, id, DbTransaction);
            CloseConnection(conn);
            return value;
        }

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
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var values = conn.Query<T>(sql, parameterObject, DbTransaction).ToList();
            CloseConnection(conn);
            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IList<TEntity> GetAll( bool isReadDb = true)
        {
            var conn = OpenDbConnection(isReadDb);
            var list = conn.GetAll<TEntity>().ToList();
            CloseConnection(conn);
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public override PagedList<TEntity> GetPagedList(string whereSql, string orderBy, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true)
        {
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
        /// <param name="sql">sql语句须确保没有多余的空格</param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override List<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true)
        {
            var conn = OpenDbConnection(isReadDb);
            var pagedList = conn.QueryPaged<T>(sql,pageIndex,pageSize, parameterObjects, DbTransaction);
            CloseConnection(conn);
            return pagedList;
        }
    }
}
