#if ASYNC
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Extension;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
        /// <param name="data"></param>
        /// <returns></returns>
        public override async Task<bool> AddAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = await conn.InsertAsync(data, DbTransaction) > 0;
            CloseConnection(conn);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public override async Task<bool> BatchAddAsync<T>(IList<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            TypeMapper.SetTypeMap(typeof(T));
            var conn = OpenDbConnection(false);
            var dt = conn.GetTableSchema(list);
            var value = await BulkToDbAsync(conn, dt, DbTransaction);
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
        private static async Task<bool> BulkToDbAsync(IDbConnection conn, DataTable dt, IDbTransaction dbTransaction)
        {
            SqlConnection sqlConn = conn as SqlConnection;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction);
            bulkCopy.DestinationTableName = dt.TableName;
            bulkCopy.BatchSize = dt.Rows.Count;

            try
            {
                if (dt.Rows.Count != 0)
                    await bulkCopy.WriteToServerAsync(dt);
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
        public override async Task<bool> SaveAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = await conn.UpdateAsync(data, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override async Task<bool> RemoveAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = await conn.DeleteAsync(data, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override async Task<int> RemoveAsync(string whereSql, object parameterObject = null)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(false);
            var value = await conn.DeleteAsync<TEntity>(whereSql, parameterObject, DbTransaction);
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
        public override async Task<TEntity> GetAsync(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var value = await conn.QueryFirstOrDefaultAsync<TEntity>(sql, parameterObject, DbTransaction);
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
        public override async Task<T> GetAsync<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            TypeMapper.SetTypeMap(typeof(T));
            var conn = OpenDbConnection(isReadDb);
            var value = await conn.QueryFirstOrDefaultAsync<T>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<TEntity> GetAsync(object condition, bool isReadDb = true)
        {
            return GetListAsync(condition, isReadDb).Result.ToList().FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<TEntity> GetByIdAsync(dynamic id, bool isReadDb = true)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var value = await SqlMapperExtensions.GetAsync<TEntity>(conn, id, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// isUpdateLock使用WITH (UPDLOCK)，其他事务可读取，不可更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isUpdateLock"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<TEntity> GetByIdAsync(dynamic id, bool isUpdateLock, bool isReadDb = true)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var value = await SqlMapperExtensions.GetAsync<TEntity>(conn, id, isUpdateLock, DbTransaction);
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
        public override async Task<IList<T>> GetListAsync<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(T));
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var values = await conn.QueryAsync<T>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<IList<TEntity>> GetListAsync(object condition, bool isReadDb = true)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            var list = await conn.QueryListAsync<TEntity>(condition, table, "*", false, null, new int?());
            return list.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task<IList<TEntity>> GetAllAsync(bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var list = await conn.GetAllAsync<TEntity>();
            CloseConnection(conn);
            return list.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<int> GetCountAsync(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var values = await conn.ExecuteScalarAsync<int>(sql, parameterObject, DbTransaction);
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
        public override async Task<PagedList<TEntity>> GetPagedListAsync(string whereSql, string orderBy, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            PagedList<TEntity> pagedList = new PagedList<TEntity>(pageIndex, pageSize, whereSql, orderBy);
            pagedList = await conn.QueryPagedAsync(pagedList, parameterObjects, DbTransaction);
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
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<List<T>> GetPagedListAsync<T>(string sql, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(T));
            var conn = OpenDbConnection(isReadDb);
            var pagedList = await conn.QueryPagedAsync<T>(sql, pageIndex, pageSize, parameterObjects, DbTransaction);
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
        public override Task<PagedList<TEntity>> GetPagedListAsync(object condition, string orderBy, int pageIndex, int pageSize, bool isReadDb = true)
        {
            TypeMapper.SetTypeMap(typeof(TEntity));
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            return conn.QueryPagedAsync<TEntity>(condition, table, orderBy, pageIndex, pageSize, "*", false, null, new int?());
        }
    }
}
#endif