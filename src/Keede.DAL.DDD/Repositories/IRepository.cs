using System.Collections.Generic;
using System.Data;
using Dapper;
using Dapper.Extension;
using System;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// Represents that the implemented classes are repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public partial interface IRepository<TEntity>: IDisposable
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
        /// <param name="destinationTableName"></param>
        /// <returns></returns>
        bool BatchAdd<T>(IList<T> list, string destinationTableName=null); 

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
        [Obsolete]
        TEntity Get(string sql,object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        T Get<T>(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果在事务内读取，会自动加上更新锁 WITH(UPDLOCK)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        TEntity GetById(dynamic id, bool isReadDb = true);

        /// <summary>
        /// 指定Id，获取一个实体对象；如果要求附带UPDLOCK更新锁，就能防止脏读数据
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        TEntity GetAndUpdateLock(object condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        int GetCount(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// Get all items from the repository by custom condition
        /// </summary>
        /// <returns></returns>
        IList<TEntity> GetAll(bool isReadDb = true);

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
        /// <param name="whereExpression"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        PagedList<TEntity> GetPagedList(Expression<Func<TEntity, bool>> whereExpression, string orderBy, int pageIndex, int pageSize, bool isReadDb = true);

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
        [Obsolete]
        IList<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        PagedList<TEntity> GetPagedList(object condition, string orderBy, int pageIndex, int pageSize,
            bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        int SaveExpression(Expression<Func<TEntity, bool>> whereExpression, dynamic data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int RemoveExpression(Expression<Func<TEntity, bool>> whereExpression);

        bool IsExistById(object condition, bool isReadDb = true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsExist(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        bool IsExist(string sql, object condition = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        int GetCount(Expression<Func<TEntity, bool>> whereExpression, bool isReadDb = true);

        int BatchUpdate<T>(IList<T> list, string updateCommandText, string destinationTableName = null, params SqlParameter[] parameters);
    }
}
