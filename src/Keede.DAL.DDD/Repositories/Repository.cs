using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper.Extension;
using Keede.DAL.RWSplitting;
using System.Linq.Expressions;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// Represents the base class for repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public abstract partial class Repository<TEntity> : IRepository<TEntity>
        where TEntity : IEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool Add(TEntity data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="destinationTableName"></param>
        /// <returns></returns>
        public abstract bool BatchAdd<T>(IList<T> list, string destinationTableName=null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool Save(TEntity data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public abstract bool Remove(TEntity condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract TEntity Get(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract TEntity GetById(dynamic id, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public abstract TEntity GetAndUpdateLock(object condition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true);

        //public abstract IList<TEntity> GetList(string where, object parameterObject = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract T Get<T>(string sql, object parameterObject = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete]
        public abstract int GetCount(string sql, object parameterObject = null, bool isReadDb = true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IList<TEntity> GetAll( bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract PagedList<TEntity> GetPagedList(Expression<Func<TEntity, bool>> whereExpression, string orderBy, int pageIndex, int pageSize, bool isReadDb = true);

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
        public abstract IList<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, string orderBy = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="data"></param>
        /// <returns></returns>


        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        public abstract int RemoveExpression(Expression<Func<TEntity, bool>> whereExpression);
        #region IDisposable Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract TEntity Get(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract IList<TEntity> GetList(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract PagedList<TEntity> GetPagedList(object condition, string orderBy, int pageIndex, int pageSize,
            bool isReadDb = true);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~Repository()
        {
            Dispose();
        }
        #endregion

        public abstract int SaveExpression(Expression<Func<TEntity, bool>> whereExpression, dynamic data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract bool IsExistById(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract bool IsExist(object condition, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="condition"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract bool IsExist(string sql, object condition = null, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract int GetCount(Expression<Func<TEntity, bool>> whereExpression, bool isReadDb = true);

        public abstract int BatchUpdate<T>(IList<T> list, string updateCommandText, string destinationTableName = null, params SqlParameter[] parameters);
    }
}
