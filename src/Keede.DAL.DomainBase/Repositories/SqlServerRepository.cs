using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Extension;
using Keede.DAL.RWSplitting;

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
            var conn = OpenDbConnection();
            var value = conn.Insert(data, DbTransaction) > 0;
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Save(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var conn = OpenDbConnection();
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
            var conn = OpenDbConnection();
            var value = conn.Delete(data, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override TEntity Get(string sql, object parameterObject = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection();
            var value = conn.QueryFirstOrDefault<TEntity>(sql, parameterObject, DbTransaction);
            CloseConnection(conn);
            return value;
        }


        public T Get<T>(string sql, object parameterObject = null)
            where T : class
        {
            //todo
            return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override TEntity GetById(dynamic id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var conn = OpenDbConnection();
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
        public override TEntity GetById(dynamic id, bool isUpdateLock)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var conn = OpenDbConnection();
            var value = SqlMapperExtensions.Get<TEntity>(conn, id, DbTransaction);
            CloseConnection(conn);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override IList<TEntity> GetList(string sql, object parameterObject = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            var conn = OpenDbConnection();
            var values = conn.Query<TEntity>(sql, parameterObject, DbTransaction).ToList();
            CloseConnection(conn);
            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IList<TEntity> GetAll()
        {
            var conn = OpenDbConnection();
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
        public override PagedList<TEntity> GetPagedList(string whereSql, string orderBy, object parameterObjects, int pageIndex, int pageSize)
        {
            var conn = OpenDbConnection();
            PagedList<TEntity> pagedList = new PagedList<TEntity>(pageIndex, pageSize, whereSql, orderBy);
            conn.QueryPaged(ref pagedList, parameterObjects, DbTransaction);
            CloseConnection(conn);
            return pagedList;
        }
    }
}
