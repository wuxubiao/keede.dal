using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Extension;
using System;

namespace Keede.DAL.DomainBase.Repositories
{
    /// <summary>
    /// Represents that the implemented classes are repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public interface IRepository<TEntity>: IDisposable
        where TEntity : IEntity
    {
        /// <summary>
        /// Add a new item into the repository
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Add(TEntity data);

        /// <summary>
        /// Save the modified item to the repository
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Save(TEntity data);

        /// <summary>
        /// Remove item from the repository by custom condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        bool Remove(TEntity condition);

        /// <summary>
        /// Get single item from the repository by custom condition
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        TEntity Get(string sql,object parameterObject = null);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果在事务内读取，会自动加上更新锁 WITH(UPDLOCK)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity GetById(dynamic id);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果要求附带UPDLOCK更新锁，就能防止脏读数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isUpdateLock"></param>
        /// <returns></returns>
        TEntity GetById(dynamic id,bool isUpdateLock);

        /// <summary>
        /// Get multi items from the repository by custom condition
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        IList<TEntity> GetList(string sql, object parameterObject = null);

        /// <summary>
        /// Get all items from the repository by custom condition
        /// </summary>
        /// <returns></returns>
        IList<TEntity> GetAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<TEntity> GetPagedList(string whereSql, string orderBy,object parameterObjects, int pageIndex, int pageSize);
    }
}
