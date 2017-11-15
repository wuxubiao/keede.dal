#if ASYNC
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper.Extension;
using Keede.DAL.RWSplitting;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// Represents the base class for repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public abstract partial class Repository<TEntity> where TEntity : IEntity
    {
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task<bool> AddAsync(TEntity data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public abstract Task<bool> BatchAddAsync<T>(IList<T> list, string destinationTableName = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task<bool> SaveAsync(TEntity data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public abstract Task<bool> RemoveAsync(TEntity condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract Task<int> RemoveAsync(string whereSql, object parameterObject = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract Task<TEntity> GetAsync(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<TEntity> GetByIdAsync(dynamic id, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public abstract Task<TEntity> GetAndUpdateLockAsync(object condition);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract Task<IList<T>> GetListAsync<T>(string sql, object parameterObject = null, bool isReadDb = true) where T : class;

        //public abstract IList<TEntity> GetList(string where, object parameterObject = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<T> GetAsync<T>(string sql, object parameterObject = null, bool isReadDb = true) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<int> GetCountAsync(string sql, object parameterObject = null, bool isReadDb = true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Task<IList<TEntity>> GetAllAsync(bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public abstract Task<PagedList<TEntity>> GetPagedListAsync(string where, string orderBy, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<IList<T>> GetPagedListAsync<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true) where T : class;
        #region IDisposable Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<TEntity> GetAsync(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<IList<TEntity>> GetListAsync(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract Task<PagedList<TEntity>> GetPagedListAsync(object condition, string orderBy, int pageIndex, int pageSize,
            bool isReadDb = true);
        #endregion
    }
}
#endif