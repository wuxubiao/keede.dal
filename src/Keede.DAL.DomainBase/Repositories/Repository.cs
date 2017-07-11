using System;
using System.Collections.Generic;
using System.Data;
using Dapper.Extension;
using Keede.DAL.RWSplitting;

namespace Keede.DAL.DomainBase.Repositories
{
    /// <summary>
    /// Represents the base class for repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : IEntity
    {
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool Add(TEntity data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public abstract bool BatchAdd<T>(IList<T> list);

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
        /// <param name="where"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract int Remove(string whereSql, object parameterObject = null);

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
        /// <param name="id"></param>
        /// <param name="isUpdateLock"></param>
        /// <returns></returns>
        public abstract TEntity GetById(dynamic id, bool isUpdateLock, bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public abstract IList<T> GetList<T>(string sql, object parameterObject = null, bool isReadDb = true) where T : class;

        //public abstract IList<TEntity> GetList(string where, object parameterObject = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract T Get<T>(string sql, object parameterObject = null, bool isReadDb = true) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        public abstract int GetCount(string sql, object parameterObject = null, bool isReadDb = true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IList<TEntity> GetAll( bool isReadDb = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="parameterObjects"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public abstract PagedList<TEntity> GetPagedList(string where, string orderBy, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true);

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
        public abstract List<T> GetPagedList<T>(string sql, object parameterObjects, int pageIndex, int pageSize, bool isReadDb = true) where T : class;
        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~Repository()
        {
            Dispose();
        }
        #endregion
    }
}
