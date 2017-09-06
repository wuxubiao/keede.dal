using System;
using System.Data;

namespace Keede.DAL.DDD.Repositories
{
    /// <summary>
    /// Represents the base class for repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate root on which the repository operations
    /// should be performed.</typeparam>
    public abstract class RepositoryWithTransaction<TEntity> : Repository<TEntity>, IRepositoryWithTransaction<TEntity>
        where TEntity : IEntity
    {
        private bool _isEnableTransaction;

        [ThreadStatic]
        static IDbTransaction _dbTransaction;

        /// <summary>
        /// DB事务对象
        /// </summary>
        public IDbTransaction DbTransaction
        {
            get
            {
                if (_dbTransaction != null && _dbTransaction.Connection == null)
                    _dbTransaction = null;
                return _dbTransaction;
            }
            protected set => _dbTransaction = value;
        }

        /// <summary>
        /// 设置DB事务对象，设置后ConnectionString会为空
        /// </summary>
        /// <param name="dbTransaction"></param>
        public IRepositoryWithTransaction<TEntity> SetDbTransaction(IDbTransaction dbTransaction)
        {
            DbTransaction = dbTransaction;
            _isEnableTransaction = true;
            return this;
        }
    }
}
