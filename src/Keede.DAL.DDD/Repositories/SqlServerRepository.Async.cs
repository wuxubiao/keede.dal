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
    public partial class SqlServerRepository<TEntity> where TEntity : class, IEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override async Task<bool> AddAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var conn = OpenDbConnection(false, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            var result = false;

            try
            {
                result = await conn.InsertAsync(data, DbTransaction) > 0;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public override async Task<bool> BatchAddAsync<T>(IList<T> list, string destinationTableName = null)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            var conn = OpenDbConnection(false, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            var result = false;

            try
            {
                var dt = conn.GetTableSchema(list);
                if (!string.IsNullOrEmpty(destinationTableName)) dt.TableName = destinationTableName;
                result = await BulkToDbAsync(conn, dt, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(false, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            var result = false;

            try
            {
                result = await conn.UpdateAsync(data, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override async Task<bool> RemoveAsync(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var conn = OpenDbConnection(false, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            var result = false;

            try
            {
                result = await conn.DeleteAsync(data, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override async Task<int> RemoveAsync(string whereSql, object parameterObject = null)
        {
            var conn = OpenDbConnection(false, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            var result = 0;

            try
            {
                result = await conn.DeleteAsync<TEntity>(whereSql, parameterObject, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            TEntity result;

            try
            {
                result = await conn.QueryFirstOrDefaultAsync<TEntity>(sql, parameterObject, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            T result;

            try
            {
                result = await conn.QueryFirstOrDefaultAsync<T>(sql, parameterObject, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override async Task<TEntity> GetAsync(object condition, bool isReadDb = true)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            TEntity result;

            try
            {
                var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
                var list = await conn.QueryListAsync<TEntity>(condition, table, "*", false, DbTransaction, 3);
                result = list.ToList().FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            TEntity result;

            try
            {
                result = await SqlMapperExtensions.GetAsync<TEntity>(conn, id, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// isUpdateLock使用WITH (UPDLOCK)，其他事务可读取，不可更新
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public override async Task<TEntity> GetAndUpdateLockAsync(object condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            var conn = OpenDbConnection(false, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            TEntity result;

            try
            {
                var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
                result = await conn.GetAndUpdateLockAsync<TEntity>(condition, table, "*", false, DbTransaction, 3);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            IList<T> result;

            try
            {
                var list = await conn.QueryAsync<T>(sql, parameterObject, DbTransaction);
                result = list.ToList();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            IList<TEntity> result;

            try
            {
                var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
                var list = await conn.QueryListAsync<TEntity>(condition, table, "*", false, DbTransaction, 3);
                result = list.ToList();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task<IList<TEntity>> GetAllAsync(bool isReadDb = true)
        {
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            IList<TEntity> result;

            try
            {
                var list = await conn.GetAllAsync<TEntity>(DbTransaction);
                result = list.ToList();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            var result = 0;

            try
            {
                sql = SqlMapperExtensions.GetSelectColumnReplaceToCount(sql);

                result = await conn.ExecuteScalarAsync<int>(sql, parameterObject, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            PagedList<TEntity> result;

            try
            {
                PagedList<TEntity> pagedList = new PagedList<TEntity>(pageIndex, pageSize, whereSql, orderBy);
                result = await conn.QueryPagedAsync(pagedList, parameterObjects, DbTransaction);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
        public override async Task<IList<T>> GetPagedListAsync<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true)
        {
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            IList<T> result;

            try
            {
                var list = await conn.QueryPagedAsync<T>(sql, pageIndex, pageSize, orderBy, parameterObjects, DbTransaction);
                result = list.ToList();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
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
        public override async Task<PagedList<TEntity>> GetPagedListAsync(object condition, string orderBy, int pageIndex, int pageSize, bool isReadDb = true)
        {
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb, SqlMapperExtensions.GetRWSplitDbName(typeof(TEntity)));
            PagedList<TEntity> result;

            try
            {
                result = await conn.QueryPagedAsync<TEntity>(condition, table, orderBy, pageIndex, pageSize, "*", false,DbTransaction, 3);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }
    }
}
#endif