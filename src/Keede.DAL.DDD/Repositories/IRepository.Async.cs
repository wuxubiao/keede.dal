#if ASYNC
using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Extension;
using System;
using System.Threading.Tasks;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// Represents that the implemented classes are repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public partial interface IRepository<TEntity> where TEntity : IEntity
    {
        /// <summary>
        /// Add a new item into the repository
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> AddAsync(TEntity data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<bool> BatchAddAsync<T>(IList<T> list, string destinationTableName = null);

        /// <summary>
        /// Save the modified item to the repository
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> SaveAsync(TEntity data);

        /// <summary>
        /// Remove item from the repository by custom condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(TEntity condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        Task<int> RemoveAsync(string whereSql, object parameterObject = null);

        /// <summary>
        /// Get single item from the repository by custom condition
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(string sql,object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果在事务内读取，会自动加上更新锁 WITH(UPDLOCK)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<TEntity> GetByIdAsync(dynamic id, bool isReadDb = true);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果要求附带UPDLOCK更新锁，就能防止脏读数据
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        Task<TEntity> GetAndUpdateLockAsync(object condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<int> GetCountAsync(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<IList<T>> GetListAsync<T>(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// Get all items from the repository by custom condition
        /// </summary>
        /// <returns></returns>
        Task<IList<TEntity>> GetAllAsync(bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PagedList<TEntity>> GetPagedListAsync(string whereSql, string orderBy,object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true);

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
        Task<IList<T>> GetPagedListAsync<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<IList<TEntity>> GetListAsync(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        Task<PagedList<TEntity>> GetPagedListAsync(object condition, string orderBy, int pageIndex, int pageSize,
            bool isReadDb = true);
    }
}
#endif