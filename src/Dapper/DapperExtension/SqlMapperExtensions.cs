using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using Dapper;
#if COREFX
using DataException = System.InvalidOperationException;
#else
using System.Threading;
#endif

namespace Framework.Core.DB.DapperExtension
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class SqlMapperExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public interface IProxy
        {
            /// <summary>
            /// 
            /// </summary>
            bool IsDirty { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public delegate string GetDatabaseTypeDelegate(IDbConnection connection);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public delegate string TableNameMapperDelegate(Type type);

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _keyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _explicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _typeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _readTypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _computedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> _getQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> _typeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        private static readonly ISqlAdapter _defaultAdapter = new SqlServerAdapter();
        private static readonly Dictionary<string, ISqlAdapter> _adapterDictionary
            = new Dictionary<string, ISqlAdapter>
            {
                {"sqlconnection", new SqlServerAdapter()},
                {"sqlceconnection", new SqlCeServerAdapter()},
                {"npgsqlconnection", new PostgresAdapter()},
                {"sqliteconnection", new SQLiteAdapter()},
                {"mysqlconnection", new MySqlAdapter()},
                {"fbconnection", new FbAdapter() }
            };

        private static List<PropertyInfo> ComputedPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (_computedProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi.ToList();
            }

            var computedProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();

            _computedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        private static List<PropertyInfo> ExplicitKeyPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (_explicitKeyProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi.ToList();
            }

            var explicitKeyProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute)).ToList();

            _explicitKeyProperties[type.TypeHandle] = explicitKeyProperties;
            return explicitKeyProperties;
        }

        private static List<PropertyInfo> KeyPropertiesCache(Type type)
        {

            IEnumerable<PropertyInfo> pi;
            if (_keyProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi.ToList();
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = allProperties.Where(p =>
            {
                return p.GetCustomAttributes(true).Any(a => a is AutoKeyAttribute);
            }).ToList();

            if (keyProperties.Count == 0)
            {
                var idProp = allProperties.FirstOrDefault(p => p.Name.ToLower() == "id");
                if (idProp != null && !idProp.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute))
                {
                    keyProperties.Add(idProp);
                }
            }

            _keyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        private static List<PropertyInfo> TypePropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pis;
            if (_typeProperties.TryGetValue(type.TypeHandle, out pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties().Where(IsWriteable).ToArray();
            _typeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        private static List<PropertyInfo> TypePropertiesCanReadCache(Type type)
        {
            IEnumerable<PropertyInfo> pis;
            if (_readTypeProperties.TryGetValue(type.TypeHandle, out pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties().Where(IsReadable).ToArray();
            _readTypeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        private static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(WriteAttribute), false).AsList();
            if (attributes.Count != 1) return true;
            var writeAttribute = (WriteAttribute)attributes[0];
            return writeAttribute.Write;
        }

        private static bool IsReadable(PropertyInfo pi)
        {
            return pi.GetCustomAttributes(typeof(IgnoreReadAttribute), false).AsList().Count == 0;
        }

        private static PropertyInfo GetSingleKey<T>(string method)
        {
            var type = typeof(T);
            var keys = KeyPropertiesCache(type);
            var explicitKeys = ExplicitKeyPropertiesCache(type);
            var keyCount = keys.Count + explicitKeys.Count;
            if (keyCount > 1)
                throw new DataException($"{method}<T> only supports an entity with a single [Key] or [ExplicitKey] property");
            if (keyCount == 0)
                throw new DataException($"{method}<T> only supports an entity with a [Key] or an [ExplicitKey] property");

            return keys.Any() ? keys.First() : explicitKeys.First();
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <code>
        /// <example>
        /// IDbConnection dbConnection = new SqlConnection(...);
        /// dbConnection.Get&lt;T&gt;(Guid.Parse("..."));//直接传递主键ID值获取信息
        /// dbConnection.Get&lt;T&gt;(Guid.Parse("..."),null,100,false);//带有事务，超时时间，是否带更新锁读取
        /// </example>
        /// </code>
        /// <returns>Entity of T</returns>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var type = typeof(T);
            string sql;
            if (!_getQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var key = GetSingleKey<T>(nameof(Get));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
                sql = $"select {columns} from {name} where {key.Name} = @id";
                _getQueries[type.TypeHandle] = sql;
            }

            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);

            T obj;

            if (type.IsInterface())
            {
                var res = connection.Query(sql, dynParms).FirstOrDefault() as IDictionary<string, object>;

                if (res == null)
                    return default(T);

                obj = ProxyGenerator.GetInterfaceProxy<T>();

                foreach (var property in TypePropertiesCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, Convert.ChangeType(val, property.PropertyType), null);
                }

                ((IProxy)obj).IsDirty = false;   //reset change tracking and return
            }
            else
            {
                obj = connection.Query<T>(sql, dynParms, transaction, commandTimeout: commandTimeout).FirstOrDefault();
            }
            return obj;
        }

        /// <summary>
        /// Returns a list of entites from table "Ts".  
        /// Id of T must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var cacheType = typeof(List<T>);

            string sql;
            if (!_getQueries.TryGetValue(cacheType.TypeHandle, out sql))
            {
                GetSingleKey<T>(nameof(GetAll));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
                sql = $"SELECT {columns} FROM " + name;
                _getQueries[cacheType.TypeHandle] = sql;
            }

            if (!type.IsInterface()) return connection.Query<T>(sql, null, transaction, commandTimeout: commandTimeout);

            var result = connection.Query(sql);
            var list = new List<T>();
            foreach (IDictionary<string, object> res in result)
            {
                var obj = ProxyGenerator.GetInterfaceProxy<T>();
                foreach (var property in TypePropertiesCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, Convert.ChangeType(val, property.PropertyType), null);
                }
                ((IProxy)obj).IsDirty = false;   //reset change tracking and return
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, string whereSql, string orderBy = "", IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var cacheType = typeof(List<T>);

            string sql;
            if (!_getQueries.TryGetValue(cacheType.TypeHandle, out sql))
            {
                GetSingleKey<T>(nameof(GetAll));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
                if (!whereSql.StartsWith("where"))
                {
                    whereSql = " WHERE " + whereSql;
                }
                sql = $"SELECT {columns} FROM " + name + whereSql + orderBy;
                _getQueries[cacheType.TypeHandle] = sql;
            }

            if (!type.IsInterface()) return connection.Query<T>(sql, null, transaction, commandTimeout: commandTimeout);

            var result = connection.Query(sql);
            var list = new List<T>();
            foreach (IDictionary<string, object> res in result)
            {
                var obj = ProxyGenerator.GetInterfaceProxy<T>();
                foreach (var property in TypePropertiesCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, Convert.ChangeType(val, property.PropertyType), null);
                }
                ((IProxy)obj).IsDirty = false;   //reset change tracking and return
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// Specify a custom table name mapper based on the POCO type name
        /// </summary>
        public static TableNameMapperDelegate TableNameMapper;

        private static string GetTableName(Type type)
        {
            string name;
            if (_typeTableName.TryGetValue(type.TypeHandle, out name)) return name;

            if (TableNameMapper != null)
            {
                name = TableNameMapper(type);
            }
            else
            {
                //NOTE: This as dynamic trick should be able to handle both our own Table-attribute as well as the one in EntityFramework 
                var tableAttr = type
#if COREFX
                    .GetTypeInfo()
#endif
                    .GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic;
                if (tableAttr != null)
                    name = tableAttr.Name;
                else
                {
                    name = type.Name + "s";
                    if (type.IsInterface() && name.StartsWith("I"))
                        name = name.Substring(1);
                }
            }

            _typeTableName[type.TypeHandle] = name;
            return name;
        }

        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id or number if inserted rows if inserting a list.
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert, can be list of entities</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <code>
        /// <example>
        /// public class Order
        /// {
        ///     public Guid OrderId{get;set;}
        ///     public string OrderNo{get;set;}
        ///     public DateTime OrderTime{get;set;}
        /// }
        /// var order = new Order
        /// {
        ///     OrderId = Guid.NewGuid(),
        ///     OrderNo = "RT12540155145",
        ///     OrderTime = DateTime.Now
        /// };
        /// IDbConnection dbConnection = new SqlConnection(...);
        /// dbConnection.Insert&lt;T&gt;(order);
        /// </example>
        /// </code>
        /// <returns>Identity of inserted entity, or number of inserted rows if inserting a list</returns>
        public static long Insert<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var isList = false;

            var type = typeof(T);

            if (type.IsArray)
            {
                isList = true;
                type = type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                isList = true;
                type = type.GetGenericArguments()[0];
            }

            var name = GetTableName(type);
            var sbColumnList = new StringBuilder(null);
            var allProperties = TypePropertiesCache(type);
            var keyProperties = KeyPropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);
            var allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties)).ToList();

            var adapter = GetFormatter(connection);

            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count; i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);
                adapter.AppendColumnName(sbColumnList, property.Name);  //fix for issue #336
                if (i < allPropertiesExceptKeyAndComputed.Count - 1)
                    sbColumnList.Append(", ");
            }

            var sbParameterList = new StringBuilder(null);
            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count; i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);
                sbParameterList.AppendFormat("@{0}", property.Name);
                if (i < allPropertiesExceptKeyAndComputed.Count - 1)
                    sbParameterList.Append(", ");
            }

            int returnVal;
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed) connection.Open();

            if (!isList)    //single entity
            {
                returnVal = adapter.Insert(connection, transaction, commandTimeout, name, sbColumnList.ToString(),
                    sbParameterList.ToString(), keyProperties, entityToInsert);
            }
            else
            {
                //insert list of entities
                var cmd = $"insert into {name} ({sbColumnList}) values ({sbParameterList})";
                returnVal = connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            }
            if (wasClosed) connection.Close();
            return returnVal;
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <code>
        /// <example>
        /// var id = Guid.Parse("...");
        /// IDbConnection dbConnection = new SqlConnection(...);
        /// var order = dbConnection.Get&lt;Order&gt;(id);
        /// order.OrderState = 5; 
        /// dbConnection.Update&lt;T&gt;(order);
        /// </example>
        /// </code>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static bool Update<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var proxy = entityToUpdate as IProxy;
            if (proxy != null)
            {
                if (!proxy.IsDirty) return false;
            }

            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                type = type.GetGenericArguments()[0];
            }

            var keyProperties = KeyPropertiesCache(type).ToList();  //added ToList() due to issue #418, must work on a list copy
            var explicitKeyProperties = ExplicitKeyPropertiesCache(type);
            if (!keyProperties.Any() && !explicitKeyProperties.Any())
                throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", name);

            var allProperties = TypePropertiesCache(type);
            keyProperties.AddRange(explicitKeyProperties);
            var computedProperties = ComputedPropertiesCache(type);
            var nonIdProps = allProperties.Except(keyProperties.Union(computedProperties)).ToList();

            var adapter = GetFormatter(connection);

            for (var i = 0; i < nonIdProps.Count; i++)
            {
                var property = nonIdProps.ElementAt(i);
                adapter.AppendColumnNameEqualsValue(sb, property.Name);  //fix for issue #336
                if (i < nonIdProps.Count - 1)
                    sb.AppendFormat(", ");
            }
            sb.Append(" where ");
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties.ElementAt(i);
                adapter.AppendColumnNameEqualsValue(sb, property.Name);  //fix for issue #336
                if (i < keyProperties.Count - 1)
                    sb.AppendFormat(" and ");
            }
            var updated = connection.Execute(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout, transaction: transaction);
            return updated > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static bool UpdateId<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction, int commandTimeout = 3)
        {
            var type = typeof(T);
            var key = GetSingleKey<T>(nameof(Get));
            var name = GetTableName(type);
            var canReadProperties = TypePropertiesCanReadCache(type);
            if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
            string sql = $"update {name} set {key.Name}=@id where {key.Name} = @id";
            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);
            return connection.Execute(sql, dynParms, transaction,commandTimeout) > 0;
        }

        /// <summary>
        /// Delete entity in table "Ts".
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <code>
        /// <example>
        /// var id = Guid.Parse("...");
        /// IDbConnection dbConnection = new SqlConnection(...);
        /// var order = dbConnection.Get&lt;Order&gt;(id);
        /// if (order != null)
        /// { 
        ///     dbConnection.Delete&lt;T&gt;(order);
        /// }
        /// </example>
        /// </code> 
        /// <returns>true if deleted, false if not found</returns>
        public static bool Delete<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (entityToDelete == null)
                throw new ArgumentException("Cannot Delete null Object", nameof(entityToDelete));

            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                type = type.GetGenericArguments()[0];
            }

            var keyProperties = KeyPropertiesCache(type).ToList();  //added ToList() due to issue #418, must work on a list copy
            var explicitKeyProperties = ExplicitKeyPropertiesCache(type);
            if (!keyProperties.Any() && !explicitKeyProperties.Any())
                throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");

            var name = GetTableName(type);
            keyProperties.AddRange(explicitKeyProperties);

            var sb = new StringBuilder();
            sb.AppendFormat("delete from {0} where ", name);

            var adapter = GetFormatter(connection);

            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties.ElementAt(i);
                adapter.AppendColumnNameEqualsValue(sb, property.Name);  //fix for issue #336
                if (i < keyProperties.Count - 1)
                    sb.AppendFormat(" and ");
            }
            var deleted = connection.Execute(sb.ToString(), entityToDelete, transaction, commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Delete all entities in the table related to the type T.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if none found</returns>
        public static bool DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = $"delete from {name}";
            var deleted = connection.Execute(statement, null, transaction, commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Specifies a custom callback that detects the database type instead of relying on the default strategy (the name of the connection type object).
        /// Please note that this callback is global and will be used by all the calls that require a database specific adapter.
        /// </summary>
        public static GetDatabaseTypeDelegate GetDatabaseType;

        private static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = GetDatabaseType?.Invoke(connection).ToLower()
                       ?? connection.GetType().Name.ToLower();

            return !_adapterDictionary.ContainsKey(name)
                ? _defaultAdapter
                : _adapterDictionary[name];
        }

        static class ProxyGenerator
        {
            private static readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();

            private static AssemblyBuilder GetAsmBuilder(string name)
            {
#if COREFX
                return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);
#else
                return Thread.GetDomain().DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);
#endif
            }

            public static T GetInterfaceProxy<T>()
            {
                Type typeOfT = typeof(T);

                Type k;
                if (_typeCache.TryGetValue(typeOfT, out k))
                {
                    return (T)Activator.CreateInstance(k);
                }
                var assemblyBuilder = GetAsmBuilder(typeOfT.Name);

                var moduleBuilder = assemblyBuilder.DefineDynamicModule("SqlMapperExtensions." + typeOfT.Name); //NOTE: to save, add "asdasd.dll" parameter

                var interfaceType = typeof(IProxy);
                var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "_" + Guid.NewGuid(),
                    TypeAttributes.Public | TypeAttributes.Class);
                typeBuilder.AddInterfaceImplementation(typeOfT);
                typeBuilder.AddInterfaceImplementation(interfaceType);

                //create our _isDirty field, which implements IProxy
                var setIsDirtyMethod = CreateIsDirtyProperty(typeBuilder);

                // Generate a field for each property, which implements the T
                foreach (var property in typeof(T).GetProperties())
                {
                    var isId = property.GetCustomAttributes(true).Any(a => a is AutoKeyAttribute);
                    CreateProperty<T>(typeBuilder, property.Name, property.PropertyType, setIsDirtyMethod, isId);
                }

#if COREFX
                var generatedType = typeBuilder.CreateTypeInfo().AsType();
#else
                var generatedType = typeBuilder.CreateType();
#endif

                _typeCache.Add(typeOfT, generatedType);
                return (T)Activator.CreateInstance(generatedType);
            }


            private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
            {
                var propType = typeof(bool);
                var field = typeBuilder.DefineField("_" + "IsDirty", propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty("IsDirty",
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new[] { propType });

                const MethodAttributes GET_SET_ATTR = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                    MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + "IsDirty",
                                             GET_SET_ATTR,
                                             propType,
                                             Type.EmptyTypes);
                var currGetIl = currGetPropMthdBldr.GetILGenerator();
                currGetIl.Emit(OpCodes.Ldarg_0);
                currGetIl.Emit(OpCodes.Ldfld, field);
                currGetIl.Emit(OpCodes.Ret);
                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + "IsDirty",
                                             GET_SET_ATTR,
                                             null,
                                             new[] { propType });
                var currSetIl = currSetPropMthdBldr.GetILGenerator();
                currSetIl.Emit(OpCodes.Ldarg_0);
                currSetIl.Emit(OpCodes.Ldarg_1);
                currSetIl.Emit(OpCodes.Stfld, field);
                currSetIl.Emit(OpCodes.Ret);

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(IProxy).GetMethod("get_" + "IsDirty");
                var setMethod = typeof(IProxy).GetMethod("set_" + "IsDirty");
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);

                return currSetPropMthdBldr;
            }

            private static void CreateProperty<T>(TypeBuilder typeBuilder, string propertyName, Type propType, MethodInfo setIsDirtyMethod, bool isIdentity)
            {
                //Define the field and the property 
                var field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(propertyName,
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new[] { propType });

                const MethodAttributes GET_SET_ATTR = MethodAttributes.Public | MethodAttributes.Virtual |
                                                    MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
                                             GET_SET_ATTR,
                                             propType,
                                             Type.EmptyTypes);

                var currGetIl = currGetPropMthdBldr.GetILGenerator();
                currGetIl.Emit(OpCodes.Ldarg_0);
                currGetIl.Emit(OpCodes.Ldfld, field);
                currGetIl.Emit(OpCodes.Ret);

                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                                             GET_SET_ATTR,
                                             null,
                                             new[] { propType });

                //store value in private field and set the isdirty flag
                var currSetIl = currSetPropMthdBldr.GetILGenerator();
                currSetIl.Emit(OpCodes.Ldarg_0);
                currSetIl.Emit(OpCodes.Ldarg_1);
                currSetIl.Emit(OpCodes.Stfld, field);
                currSetIl.Emit(OpCodes.Ldarg_0);
                currSetIl.Emit(OpCodes.Ldc_I4_1);
                currSetIl.Emit(OpCodes.Call, setIsDirtyMethod);
                currSetIl.Emit(OpCodes.Ret);

                //TODO: Should copy all attributes defined by the interface?
                if (isIdentity)
                {
                    var keyAttribute = typeof(AutoKeyAttribute);
                    var myConstructorInfo = keyAttribute.GetConstructor(new Type[] { });
                    var attributeBuilder = new CustomAttributeBuilder(myConstructorInfo, new object[] { });
                    property.SetCustomAttribute(attributeBuilder);
                }

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(T).GetMethod("get_" + propertyName);
                var setMethod = typeof(T).GetMethod("set_" + propertyName);
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);
            }
        }
    }

    #region -- Attribute

    /// <summary>
    /// 自定义数据表名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tableName"></param>
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }
    }

    /// <summary>
    /// 自动主键KEY，在Insert是会被过滤掉，适合用于自增主键和数据库自动赋值主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// 明确主键，可用于Insert时添加进去
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExplicitKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// 标识写入，传递false无法Insert和Update
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WriteAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="write"></param>
        public WriteAttribute(bool write)
        {
            Write = write;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool Write { get; }
    }

    /// <summary>
    /// 标识已计算过，此特性在Insert和Update都会被过滤掉
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComputedAttribute : Attribute
    {
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreReadAttribute : Attribute
    {
    }

    #endregion
}

#region -- SqlAdapter
/// <summary>
/// 
/// </summary>
public interface ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    void AppendColumnName(StringBuilder sb, string columnName);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    void AppendColumnNameEqualsValue(StringBuilder sb, string columnName);
}

/// <summary>
/// 
/// </summary>
public class SqlServerAdapter : ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"insert into {tableName} ({columnList}) values ({parameterList});select SCOPE_IDENTITY() id";
        var multi = connection.QueryMultiple(cmd, entityToInsert, transaction, commandTimeout);

        var first = multi.Read().FirstOrDefault();
        if (first == null || first.id == null) return 0;

        var id = (int)first.id;
        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        if (!propertyInfos.Any()) return id;

        var idProperty = propertyInfos.First();
        idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

        return id;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}]", columnName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}] = @{1}", columnName, columnName);
    }
}
/// <summary>
/// 
/// </summary>
public class SqlCeServerAdapter : ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
        connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
        var r = connection.Query("select @@IDENTITY id", transaction: transaction, commandTimeout: commandTimeout).ToList();

        if (r.First().id == null) return 0;
        var id = (int)r.First().id;

        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        if (!propertyInfos.Any()) return id;

        var idProperty = propertyInfos.First();
        idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

        return id;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}]", columnName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}] = @{1}", columnName, columnName);
    }
}
/// <summary>
/// 
/// </summary>
public class MySqlAdapter : ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
        connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
        var r = connection.Query("Select LAST_INSERT_ID() id", transaction: transaction, commandTimeout: commandTimeout);

        var id = r.First().id;
        if (id == null) return 0;
        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        if (!propertyInfos.Any()) return Convert.ToInt32(id);

        var idp = propertyInfos.First();
        idp.SetValue(entityToInsert, Convert.ChangeType(id, idp.PropertyType), null);

        return Convert.ToInt32(id);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("`{0}`", columnName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("`{0}` = @{1}", columnName, columnName);
    }
}
/// <summary>
/// 
/// </summary>
public partial class PostgresAdapter : ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

        // If no primary key then safe to assume a join table with not too much data to return
        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        if (!propertyInfos.Any())
            sb.Append(" RETURNING *");
        else
        {
            sb.Append(" RETURNING ");
            var first = true;
            foreach (var property in propertyInfos)
            {
                if (!first)
                    sb.Append(", ");
                first = false;
                sb.Append(property.Name);
            }
        }

        var results = connection.Query(sb.ToString(), entityToInsert, transaction, commandTimeout: commandTimeout).ToList();

        // Return the key by assinging the corresponding property in the object - by product is that it supports compound primary keys
        var id = 0;
        foreach (var p in propertyInfos)
        {
            var value = ((IDictionary<string, object>)results.First())[p.Name.ToLower()];
            p.SetValue(entityToInsert, value, null);
            if (id == 0)
                id = Convert.ToInt32(value);
        }
        return id;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\"", columnName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
    }
}
/// <summary>
/// 
/// </summary>
public partial class SQLiteAdapter : ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList}); SELECT last_insert_rowid() id";
        var multi = connection.QueryMultiple(cmd, entityToInsert, transaction, commandTimeout);

        var id = (int)multi.Read().First().id;
        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        if (!propertyInfos.Any()) return id;

        var idProperty = propertyInfos.First();
        idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

        return id;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\"", columnName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
    }
}
/// <summary>
/// 
/// </summary>
public partial class FbAdapter : ISqlAdapter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="tableName"></param>
    /// <param name="columnList"></param>
    /// <param name="parameterList"></param>
    /// <param name="keyProperties"></param>
    /// <param name="entityToInsert"></param>
    /// <returns></returns>
    public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
    {
        var cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
        connection.Execute(cmd, entityToInsert, transaction, commandTimeout);

        var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
        var keyName = propertyInfos.First().Name;
        var r = connection.Query($"SELECT FIRST 1 {keyName} ID FROM {tableName} ORDER BY {keyName} DESC", transaction: transaction, commandTimeout: commandTimeout);

        var id = r.First().ID;
        if (id == null) return 0;
        if (!propertyInfos.Any()) return Convert.ToInt32(id);

        var idp = propertyInfos.First();
        idp.SetValue(entityToInsert, Convert.ChangeType(id, idp.PropertyType), null);

        return Convert.ToInt32(id);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("{0}", columnName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="columnName"></param>
    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("{0} = @{1}", columnName, columnName);
    }
}
#endregion