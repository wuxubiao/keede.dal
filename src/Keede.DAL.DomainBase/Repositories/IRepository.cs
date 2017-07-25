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
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool BatchAdd<T>(IList<T> list);

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
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        int Remove(string whereSql, object parameterObject = null);

        /// <summary>
        /// Get single item from the repository by custom condition
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        TEntity Get(string sql,object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
         T Get<T>(string sql, object parameterObject = null, bool isReadDb = true) where T : class;

        /// <summary>
        /// 指定Id，获取一个实体对象；如果在事务内读取，会自动加上更新锁 WITH(UPDLOCK)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity GetById(dynamic id, bool isReadDb = true);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果要求附带UPDLOCK更新锁，就能防止脏读数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isUpdateLock"></param>
        /// <returns></returns>
        TEntity GetById(dynamic id,bool isUpdateLock, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        int GetCount(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true) where T : class;

        /// <summary>
        /// Get all items from the repository by custom condition
        /// </summary>
        /// <returns></returns>
        IList<TEntity> GetAll(bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<TEntity> GetPagedList(string whereSql, string orderBy,object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true);

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
        List<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        TEntity Get(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        IList<TEntity> GetList(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        PagedList<TEntity> PageList(object condition, string orderBy, int pageIndex, int pageSize,
            bool isReadDb = true);
    }
}
