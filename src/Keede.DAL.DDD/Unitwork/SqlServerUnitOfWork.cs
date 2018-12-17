using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Keede.DAL.DDD.Repositories;
using Keede.DAL.Utility;
using Keede.DAL.RWSplitting;

namespace Keede.DAL.DDD.Unitwork
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerUnitOfWork : UnitOfWork
    {
        private readonly string _dbConnectString;
        private IDbConnection _openedConnect;
        private IDbTransaction _openedTransaction;

        /// <summary>
        /// 全局缓存，缓存仓储的构造函数
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConstructorInfo> _constructorDic = new ConcurrentDictionary<string, ConstructorInfo>();

        /// <summary>
        /// 临时缓存，记录当前工作单元上下文中用到的仓储
        /// </summary>
        private readonly Dictionary<string, dynamic> _repositoriesDic = new Dictionary<string, dynamic>();

        private static readonly ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler> _addMethodDic = new ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler>();
        private static readonly ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler> _saveMethodDic = new ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler>();
        private static readonly ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler> _removeMethodDic = new ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler>();
        private static readonly ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler> _customMethodDic = new ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler>();

        private static readonly ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler> _removeExpressionMethodDic = new ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler>();
        private static readonly ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler> _saveExpressionMethodDic = new ConcurrentDictionary<string, FastMethodUtility.FastInvokeHandler>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityAssembly"></param>
        /// <param name="repositoryAssembly"></param>
        public static void InitRepository(Assembly entityAssembly, Assembly repositoryAssembly)
        {
              var entityTypes = entityAssembly.GetTypes().Where(d => d.IsClass && !d.IsAbstract)
                .Where(t => t.GetInterfaces().Any(d => d.Name == "IEntity"));

            var repositoryTypess= repositoryAssembly.GetTypes().Where(d => d.IsClass && !d.IsAbstract)
                .Where(t => t.GetInterfaces().Any(d => d.IsGenericType && d.GetGenericTypeDefinition() == typeof(IRepository<>)));

            foreach (var entityType in entityTypes)
            {
                Type realType = MakeRealType(entityType);

                foreach (var type in repositoryTypess.Where(type => realType.IsAssignableFrom(type)))
                {
                    var constructor = type.GetConstructor(new Type[0]);
                    _constructorDic.AddOrUpdate(realType.FullName, constructor, (key, existed) => constructor);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectString"></param>
        public SqlServerUnitOfWork(string connectString)
        {
            _dbConnectString = connectString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReadDb"></param>
        public SqlServerUnitOfWork(bool isReadDb = false)
        {
            _dbConnectString=Databases.GetDbConnectionStr(isReadDb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="isReadDb"></param>
        public SqlServerUnitOfWork(string dbName, bool isReadDb = false)
        {
            _dbConnectString=Databases.GetDbConnectionStr(dbName, isReadDb);
        }

        /// <summary>
        /// 
        /// </summary>

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Commit()
        {
            if (Committed)
            {
                return false;
            }

            bool isSuccess;
            if (_openedConnect != null)
            {
                isSuccess = UseCreatedConnectCommit();
            }
            else
            {
                isSuccess = CreateNewConnectCommit();
            }

            OnCommittedEvent(isSuccess);

            return isSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Rollback()
        {
            Committed = false;
            _openedConnect?.Dispose();
            _openedTransaction?.Dispose();
            _openedConnect = null;
            _openedTransaction = null;

            try
            {
                OnRollBackEvent();
            }
            catch
            {
                // ignored
            }
            finally
            {
                ClearRegistrations();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IDbTransaction DbTransaction => _openedTransaction;

        /// <summary>
        /// 
        /// </summary>
        public override IDbConnection DbConnection => _openedConnect;

        /// <summary>
        /// 开启事务
        /// </summary>
        public override void BeginTransaction()
        {
            CheckCreatedConnectAndEnableTransaction();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Committed)
                    Rollback();
                base.Dispose(true);
            }
        }

        /// <summary>
        /// 检查已创建的连接和事务
        /// </summary>
        private void CheckCreatedConnectAndEnableTransaction(bool isEnableTransaction=true)
        {
            if (_openedConnect == null)
            {
                Interlocked.CompareExchange(ref _openedConnect, new SqlConnection(_dbConnectString), null);
            }

            if (_openedConnect.State == ConnectionState.Broken || _openedConnect.State == ConnectionState.Closed)
            {
                if (string.IsNullOrWhiteSpace(_openedConnect.ConnectionString))
                {
                    _openedConnect.ConnectionString = _dbConnectString;
                }
                _openedConnect.Open();
            }
            if (isEnableTransaction && _openedTransaction == null) _openedTransaction = _openedConnect.BeginTransaction();
        }

        /// <summary>
        /// 使用已创建的连接进行提交
        /// </summary>
        /// <returns></returns>
        private bool UseCreatedConnectCommit()
        {

            using (_openedConnect)
            {
                CheckCreatedConnectAndEnableTransaction();
                using (_openedTransaction)
                {
                    try
                    {
                        foreach (var deletedData in DeletedCollection)
                        {
                            var repository = GetRepostiroy(deletedData.Value, _openedTransaction);
                            var removeMethod = GetRemoveMethod(repository);
                            removeMethod.Invoke(repository, new[] { deletedData.Value, null });
                        }

                        foreach (var newData in NewCollection)
                        {
                            var repository = GetRepostiroy(newData.Value, _openedTransaction);
                            var addMethod = GetAddMethod(repository);
                            addMethod.Invoke(repository, new[] { newData.Value, null });
                        }

                        foreach (var modifiedData in ModifiedCollection)
                        {
                            var repository = GetRepostiroy(modifiedData.Value, _openedTransaction);
                            var saveMethod = GetSaveMethod(repository);
                            saveMethod.Invoke(repository, new[] { modifiedData.Value, null });
                        }

                        foreach (var customOperate in CustomOperateCollection)
                        {
                            var repository = GetRepostiroy(customOperate.Value.RepositoryItemType, _openedTransaction);
                            var method = GetCustomMethod(repository, customOperate.Value.OperateName);
                            method.Invoke(repository, new[] { customOperate.Value.Data });
                        }

                        foreach (var modifiedData in ExpressionModifiedCollection)
                        {
                            var repository = GetRepostiroy(modifiedData.Value.EntityType, _openedTransaction);
                            var modifiedExpressionMethod = GetModifiedExpressionMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler)modifiedExpressionMethod).Invoke(repository, new object[] { modifiedData.Value.WhereExpression,
                                modifiedData.Value.Data, null });
                        }

                        foreach (var deletedData in ExpressionDeletedCollection)
                        {
                            var repository = GetRepostiroy(deletedData.Value.EntityType, _openedTransaction);
                            var removeExpressionMethod = GetRemoveExpressionMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler)removeExpressionMethod).Invoke(repository, new object[] { deletedData.Value.WhereExpression, null });
                        }

                        _openedTransaction.Commit();
                        Committed = true;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _openedTransaction.Rollback();
                        Committed = false;
                        throw ex;
                    }
                    finally
                    {
                        ClearRegistrations();
                    }
                }
            }
        }

        /// <summary>
        /// 创建新连接进行提交
        /// </summary>
        /// <returns></returns>
        private bool CreateNewConnectCommit()
        {
            using (var connection = new SqlConnection(_dbConnectString))
            {
                connection.Open();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var deletedData in DeletedCollection)
                        {
                            var repository = GetRepostiroy(deletedData.Value, trans);
                            var removeMethod = GetRemoveMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler) removeMethod).Invoke(repository,
                                new object[] {deletedData.Value, null});
                        }

                        foreach (var customOperate in CustomOperateCollection)
                        {
                            var repository = GetRepostiroy(customOperate.Value.RepositoryItemType, trans);
                            var method = GetCustomMethod(repository, customOperate.Value.OperateName);
                            ((FastMethodUtility.FastInvokeHandler) method).Invoke(repository,
                                new object[] {customOperate.Value.Data});
                        }

                        foreach (var newData in NewCollection)
                        {
                            var repository = GetRepostiroy(newData.Value, trans);
                            var addMethod = GetAddMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler) addMethod).Invoke(repository,
                                new object[] {newData.Value, null});
                        }

                        foreach (var modifiedData in ModifiedCollection)
                        {
                            var repository = GetRepostiroy(modifiedData.Value, trans);
                            var saveMethod = GetSaveMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler) saveMethod).Invoke(repository,
                                new object[] {modifiedData.Value, null});
                        }

                        foreach (var modifiedData in ExpressionModifiedCollection)
                        {
                            var repository = GetRepostiroy(modifiedData.Value.EntityType, trans);
                            var modifiedExpressionMethod = GetModifiedExpressionMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler) modifiedExpressionMethod).Invoke(repository,
                                new object[]
                                {
                                    modifiedData.Value.WhereExpression,
                                    modifiedData.Value.Data, null
                                });
                        }

                        foreach (var deletedData in ExpressionDeletedCollection)
                        {
                            var repository = GetRepostiroy(deletedData.Value.EntityType, trans);
                            var removeExpressionMethod = GetRemoveExpressionMethod(repository);
                            ((FastMethodUtility.FastInvokeHandler) removeExpressionMethod).Invoke(repository,
                                new object[] {deletedData.Value.WhereExpression, null});
                        }

                        trans.Commit();
                        Committed = true;
                        return true;
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        Committed = false;
                        throw;
                    }
                    finally
                    {
                        ClearRegistrations();
                    }
                }
            }
        }

        private dynamic GetRepostiroy(IEntity entity, IDbTransaction sqlTransaction)
        {
            return GetRepostiroy(entity.GetType(), sqlTransaction);
        }

        private dynamic GetRepostiroy(Type entityType, IDbTransaction sqlTransaction)
        {
            var type = entityType;
            dynamic repository;
            if (_repositoriesDic.TryGetValue(type.FullName, out repository))
                return repository;

            var constructor = GetRepostiroyConstructorInfo(entityType);
            var instance = constructor.Invoke(null);
            repository = Convert.ChangeType(instance, constructor.DeclaringType);
            repository.SetDbTransaction(sqlTransaction);
            _repositoriesDic.Add(type.FullName, repository);
            return repository;
        }

        private ConstructorInfo GetRepostiroyConstructorInfo(Type entityType)
        {
            Type realType = MakeRealType(entityType);
            if (_constructorDic.TryGetValue(realType.FullName, out ConstructorInfo constructor))
            {
                return constructor;
            }

            //找不到实体对应的实现类，则返回默认实现类
            return GetDefaultRepostiroyConstructorInfo(entityType);
        }

        /// <summary>
        /// 默认实现类构造函数元数据
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private ConstructorInfo GetDefaultRepostiroyConstructorInfo(Type entityType)
        {
            Type realType = MakeRealType(entityType);
            var constructor = realType.GetConstructor(new Type[0]);
            _constructorDic.AddOrUpdate(realType.FullName, constructor, (key, existed) => constructor);

            return constructor;
        }

        private static Type MakeRealType(Type entityType)
        {
            Type generic = typeof(SqlServerRepository<>);
            Type[] typeArgs = { entityType };
            Type realType = generic.MakeGenericType(typeArgs);

            return realType;
        }

        private FastMethodUtility.FastInvokeHandler GetAddMethod(dynamic repository)
        {
            Type repositoryType = repository.GetType();
            FastMethodUtility.FastInvokeHandler method;
            if (_addMethodDic.TryGetValue(repositoryType.FullName, out method))
                return method;

            MethodInfo addMethod = repositoryType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
            method = FastMethodUtility.GetMethodInvoker(addMethod);
            _addMethodDic.AddOrUpdate(repositoryType.FullName, method, (key, existed) => method);
            return method;
        }

        private FastMethodUtility.FastInvokeHandler GetSaveMethod(dynamic repository)
        {
            Type repositoryType = repository.GetType();
            FastMethodUtility.FastInvokeHandler method;
            if (_saveMethodDic.TryGetValue(repositoryType.FullName, out method))
                return method;

            MethodInfo saveMethod = repositoryType.GetMethod("Save", BindingFlags.Public | BindingFlags.Instance);
            method = FastMethodUtility.GetMethodInvoker(saveMethod);
            _saveMethodDic.AddOrUpdate(repositoryType.FullName, method, (key, existed) => method);
            return method;
        }

        private FastMethodUtility.FastInvokeHandler GetRemoveMethod(dynamic repository)
        {
            Type repositoryType = repository.GetType();
            FastMethodUtility.FastInvokeHandler method;
            if (_removeMethodDic.TryGetValue(repositoryType.FullName, out method))
                return method;

            MethodInfo removeMethod = repositoryType.GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance);
            method = FastMethodUtility.GetMethodInvoker(removeMethod);
            _removeMethodDic.AddOrUpdate(repositoryType.FullName, method, (key, existed) => method);
            return method;
        }

        private FastMethodUtility.FastInvokeHandler GetCustomMethod(dynamic repository, string methodName)
        {
            Type repositoryType = repository.GetType();
            FastMethodUtility.FastInvokeHandler method;
            if (_customMethodDic.TryGetValue(repositoryType.FullName + methodName, out method))
                return method;

            MethodInfo customMethod = repositoryType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            method = FastMethodUtility.GetMethodInvoker(customMethod);
            _customMethodDic.AddOrUpdate(repositoryType.FullName + methodName, method, (key, existed) => method);
            return method;
        }
        
        private FastMethodUtility.FastInvokeHandler GetModifiedExpressionMethod(dynamic repository)
        {
            Type repositoryType = repository.GetType();
            FastMethodUtility.FastInvokeHandler method;
            if (_saveExpressionMethodDic.TryGetValue(repositoryType.FullName, out method))
                return method;

            MethodInfo saveMethod = repositoryType.GetMethod("SaveExpression", BindingFlags.Public | BindingFlags.Instance);
            method = FastMethodUtility.GetMethodInvoker(saveMethod);
            _saveExpressionMethodDic.AddOrUpdate(repositoryType.FullName, method, (key, existed) => method);
            return method;
        }

        private FastMethodUtility.FastInvokeHandler GetRemoveExpressionMethod(dynamic repository)
        {
            Type repositoryType = repository.GetType();
            FastMethodUtility.FastInvokeHandler method;
            if (_removeExpressionMethodDic.TryGetValue(repositoryType.FullName, out method))
                return method;

            MethodInfo removeMethod = repositoryType.GetMethod("RemoveExpression", BindingFlags.Public | BindingFlags.Instance);
            method = FastMethodUtility.GetMethodInvoker(removeMethod);
            _removeExpressionMethodDic.AddOrUpdate(repositoryType.FullName, method, (key, existed) => method);
            return method;
        }

    }
}
