using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Extension;
using Keede.DAL.RWSplitting;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class SqlServerRepository<TEntity> : RepositoryWithTransaction<TEntity>
        where TEntity : class, IEntity
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger("SqlServerRepository");

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

            if (conn?.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn?.Dispose();
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
            var result = false;

            try
            {
                result = conn.Insert(data, DbTransaction) > 0;
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        /// <param name="destinationTableName"></param>
        /// <returns></returns>
        public override bool BatchAdd<T>(IList<T> list, string destinationTableName = null)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            var conn = OpenDbConnection(false);
            var result = false;

            try
            {
                var dt = conn.GetTableSchema(list);
                if (!string.IsNullOrEmpty(destinationTableName)) dt.TableName = destinationTableName;
                result = BulkToDb(conn, dt, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        /// <param name="updateCommandText"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override int BatchUpdate<T>(IList<T> list, string updateCommandText, string destinationTableName = null, params SqlParameter[] parameters)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            var conn = OpenDbConnection(false);
            int result;

            try
            {
                var cmd = new SqlCommand(updateCommandText, (SqlConnection)conn, (SqlTransaction)DbTransaction)
                    { CommandTimeout = int.MaxValue };
                cmd.Parameters.AddRange(parameters);

                var dt = conn.GetTableSchema(list);
                if (!string.IsNullOrEmpty(destinationTableName)) dt.TableName = destinationTableName;

                using (var adapter = new SqlDataAdapter { UpdateCommand = cmd })
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr.RowState == DataRowState.Unchanged)
                            dr.SetModified();
                    }
                    result = adapter.Update(dt);
                    dt.AcceptChanges();
                }
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        private static bool BulkToDb(IDbConnection conn, DataTable dt, IDbTransaction dbTransaction)
        {
            SqlConnection sqlConn = conn as SqlConnection;
            SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction);
            bulkCopy.DestinationTableName = dt.TableName;
            bulkCopy.BatchSize = dt.Rows.Count;

            foreach (var item in dt.Columns)
            {
                var col = (DataColumn) item;
                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            try
            {
                if (dt.Rows.Count != 0)
                    bulkCopy.WriteToServer(dt);
                return true;
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
            var conn = OpenDbConnection(false);
            var result = false;

            try
            {
                result = conn.Update(data, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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

        public override int SaveExpression(Expression<Func<TEntity, bool>> whereExpression, dynamic data)
        {
            var conn = OpenDbConnection(false);
            var result = 0;

            try
            {
                 result=SqlMapperExtensions.Update(conn, data, whereExpression, "", null, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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

        public override int RemoveExpression(Expression<Func<TEntity, bool>> whereExpression)
        {
            var conn = OpenDbConnection(false);
            var result = 0;

            try
            {
                result = conn.Delete(whereExpression, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        public override bool Remove(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var conn = OpenDbConnection(false);
            var result = false;

            try
            {
                result = conn.Delete(data, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        [Obsolete]
        public override TEntity Get(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var result = default(TEntity);

            try
            {
                result = conn.QueryFirstOrDefault<TEntity>(sql, parameterObject, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        [Obsolete]
        public override T Get<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var result = default(T);

            try
            {
                result = conn.QueryFirstOrDefault<T>(sql, parameterObject, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
            var conn = OpenDbConnection(isReadDb);
            var result = default(TEntity);

            try
            {
                result = SqlMapperExtensions.Get<TEntity>(conn, id, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        /// <returns></returns>
        public override TEntity GetAndUpdateLock(object condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            var conn = OpenDbConnection(false);
            var result = default(TEntity);

            try
            {
                var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
                result = conn.GetAndUpdateLock<TEntity>(condition, table, "*", false, DbTransaction, 3);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        [Obsolete]
        public override IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var result = default(IList<T>);

            try
            {
                result = conn.Query<T>(sql, parameterObject, DbTransaction).ToList();
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        public override IList<TEntity> GetList(object condition, bool isReadDb = true)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            var conn = OpenDbConnection(isReadDb);
            var result = default(IList<TEntity>);

            try
            {
                var tableName = SqlMapperExtensions.GetTableName(typeof(TEntity));
                result = conn.QueryList<TEntity>(condition, tableName, "*", false, DbTransaction, 3).ToList();
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        public override IList<TEntity> GetAll(bool isReadDb = true)
        {
            var conn = OpenDbConnection(isReadDb);
            var result = default(IList<TEntity>);

            try
            {
                result = conn.GetAll<TEntity>(DbTransaction).ToList();
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        [Obsolete]
        public override int GetCount(string sql, object parameterObject = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var result = 0;

            try
            {
                sql = SqlMapperExtensions.GetSelectColumnReplaceToCount(sql);

                result = (int)conn.ExecuteScalar(sql, parameterObject, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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

        public override int GetCount(Expression<Func<TEntity, bool>> whereExpression, bool isReadDb = true)
        {
            var translate = new SqlTranslateFormater();
            string whereSql = translate.Translate(whereExpression);

            var entityType = whereExpression.Parameters[0].Type;
            var tableName = SqlMapperExtensions.GetTableName(entityType);
            string sql = "select count(*) from "+ tableName+ " where " + whereSql;

            var conn = OpenDbConnection(isReadDb);
            var result = 0;

            try
            {
                result = (int)conn.ExecuteScalar(sql, null, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        /// <param name="whereExpression"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override PagedList<TEntity> GetPagedList(Expression<Func<TEntity, bool>> whereExpression, string orderBy, int pageIndex, int pageSize, bool isReadDb = true)
        {
            var translate = new SqlTranslateFormater();
            string whereSql = translate.Translate(whereExpression);

            var result = new PagedList<TEntity>(pageIndex, pageSize, whereSql, orderBy);
            var conn = OpenDbConnection(isReadDb);

            try
            {
                conn.QueryPaged(ref result, null, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy">
        /// orderBy为空则去sql语句中的order by，sql语句无order by则默认ORDER BY getdate()
        /// </param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        public override IList<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true)
        {
            var conn = OpenDbConnection(isReadDb);
            var result = default(IList<T>);

            try
            {
                result = conn.QueryPaged<T>(sql, pageIndex, pageSize, orderBy, parameterObjects, DbTransaction);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        public override PagedList<TEntity> GetPagedList(object condition, string orderBy, int pageIndex, int pageSize, bool isReadDb = true)
        {
            var table = SqlMapperExtensions.GetTableName(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var result = default(PagedList<TEntity>);

            try
            {
                result = conn.QueryPaged<TEntity>(condition, table, orderBy, pageIndex, pageSize, "*", false, DbTransaction, 3);
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        public override bool IsExistById(object id, bool isReadDb = true)
        {
            return GetById(id, isReadDb) != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public override bool IsExist(object condition, bool isReadDb = true)
        {
            var tableName = SqlMapperExtensions.GetTableName(typeof(TEntity));
            var conn = OpenDbConnection(isReadDb);
            var result = false;

            try
            {
                result = conn.GetCount(condition, tableName, false, DbTransaction) > 0;
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        public override bool IsExist(string sql, object condition = null, bool isReadDb = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection(isReadDb);
            var result = false;

            try
            {
                var count=conn.ExecuteScalar(sql, condition, DbTransaction);
                if (count == null)
                    return false;
                result = (int)count > 0;
            }
            catch (SqlStatementException statementEx)
            {
                _logger.Error(statementEx);
                throw;
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
