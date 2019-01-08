/*********************************************
https://github.com/StackExchange/Dapper/tree/master/Dapper.Contrib
*******************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection.Emit;

using Dapper;

#if NETSTANDARD1_3
using DataException = System.InvalidOperationException;
#else
using System.Threading;
#endif

namespace Dapper.Extension
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

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ExplicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();
        private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary
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
            if (ComputedProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi.ToList();
            }

            var computedProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();

            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        private static List<PropertyInfo> ExplicitKeyPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi.ToList();
            }

            var explicitKeyProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute)).ToList();

            ExplicitKeyProperties[type.TypeHandle] = explicitKeyProperties;
            return explicitKeyProperties;
        }

        public static List<PropertyInfo> KeyPropertiesCache(Type type)
        {

            IEnumerable<PropertyInfo> pi;
            if (KeyProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi.ToList();
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = allProperties.Where(p =>
            {
                return p.GetCustomAttributes(true).Any(a => a is KeyAttribute);
            }).ToList();

            if (keyProperties.Count == 0)
            {
                var idProp = allProperties.FirstOrDefault(p => GetCustomColumnName(p).ToLower() == "id");
                if (idProp != null && !idProp.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute))
                {
                    keyProperties.Add(idProp);
                }
            }

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        private static List<PropertyInfo> TypePropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pis;
            if (TypeProperties.TryGetValue(type.TypeHandle, out pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties().Where(IsWriteable).ToArray();
            TypeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        private static bool IsWriteable(PropertyInfo pi)
        {
            return pi.GetCustomAttributes(typeof(IgnoreWriteAttribute), false).AsList().Count == 0;

            //var attributes = pi.GetCustomAttributes(typeof(WriteAttribute), false).AsList();
            //if (attributes.Count != 1) return true;
            //var writeAttribute = (WriteAttribute)attributes[0];
            //return writeAttribute.Write;
        }

        private static bool IsReadable(PropertyInfo pi)
        {
            return pi.GetCustomAttributes(typeof(IgnoreReadAttribute), false).AsList().Count == 0;
        }

        /// <summary>
        /// Specify a custom table name mapper based on the POCO type name
        /// </summary>
        public static TableNameMapperDelegate TableNameMapper;

        public static string GetTableName(Type type)
        {
            string name;
            if (TypeTableName.TryGetValue(type.TypeHandle, out name)) return name;

            if (TableNameMapper != null)
            {
                name = TableNameMapper(type);
            }
            else
            {
                //NOTE: This as dynamic trick should be able to handle both our own Table-attribute as well as the one in EntityFramework 
                var tableAttr = type
#if NETSTANDARD1_3
                    .GetTypeInfo()
#endif
                    .GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic;
                if (tableAttr != null)
                    name = tableAttr.Name;
                else
                {
                    name = type.Name;
                    if (type.IsInterface() && name.StartsWith("I"))
                        name = name.Substring(1);
                }
            }

            TypeTableName[type.TypeHandle] = name;
            return name;
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
            var statement = $"delete from [{name}]";
            var deleted = connection.Execute(statement, null, transaction, commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Specifies a custom callback that detects the database type instead of relying on the default strategy (the name of the connection type object).
        /// Please note that this callback is global and will be used by all the calls that require a database specific adapter.
        /// </summary>
        public static GetDatabaseTypeDelegate GetDatabaseType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = GetDatabaseType?.Invoke(connection).ToLower()
                       ?? connection.GetType().Name.ToLower();

            return !AdapterDictionary.ContainsKey(name)
                ? DefaultAdapter
                : AdapterDictionary[name];
        }

        /// <summary>
        /// 
        /// </summary>
        private static class ProxyGenerator
        {
            private static readonly Dictionary<Type, Type> TypeCache = new Dictionary<Type, Type>();

            private static AssemblyBuilder GetAsmBuilder(string name)
            {
#if NETSTANDARD1_3 || NETSTANDARD2_0
                return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);
#else
                return Thread.GetDomain().DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);
#endif
            }

            public static T GetInterfaceProxy<T>()
            {
                Type typeOfT = typeof(T);

                if (TypeCache.TryGetValue(typeOfT, out Type k))
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
                    var isId = property.GetCustomAttributes(true).Any(a => a is KeyAttribute);
                    CreateProperty<T>(typeBuilder, property.Name, property.PropertyType, setIsDirtyMethod, isId);
                }

#if NETSTANDARD1_3 || NETSTANDARD2_0
                var generatedType = typeBuilder.CreateTypeInfo().AsType();
#else
                var generatedType = typeBuilder.CreateType();
#endif

                TypeCache.Add(typeOfT, generatedType);
                return (T)Activator.CreateInstance(generatedType);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="typeBuilder"></param>
            /// <returns></returns>
            private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
            {
                var propType = typeof(bool);
                var field = typeBuilder.DefineField("_" + "IsDirty", propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty("IsDirty",
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                                                    MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + "IsDirty",
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);
                var currGetIl = currGetPropMthdBldr.GetILGenerator();
                currGetIl.Emit(OpCodes.Ldarg_0);
                currGetIl.Emit(OpCodes.Ldfld, field);
                currGetIl.Emit(OpCodes.Ret);
                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + "IsDirty",
                                             getSetAttr,
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

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="typeBuilder"></param>
            /// <param name="propertyName"></param>
            /// <param name="propType"></param>
            /// <param name="setIsDirtyMethod"></param>
            /// <param name="isIdentity"></param>
            private static void CreateProperty<T>(TypeBuilder typeBuilder, string propertyName, Type propType, MethodInfo setIsDirtyMethod, bool isIdentity)
            {
                //Define the field and the property 
                var field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(propertyName,
                                               System.Reflection.PropertyAttributes.None,
                                               propType,
                                               new[] { propType });

                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual |
                                                    MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName,
                                             getSetAttr,
                                             propType,
                                             Type.EmptyTypes);

                var currGetIl = currGetPropMthdBldr.GetILGenerator();
                currGetIl.Emit(OpCodes.Ldarg_0);
                currGetIl.Emit(OpCodes.Ldfld, field);
                currGetIl.Emit(OpCodes.Ret);

                var currSetPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                                             getSetAttr,
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
                    var keyAttribute = typeof(KeyAttribute);
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
    /// 如不打TableAttribute标签，则会把类名+字母s作为表名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 
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
    public class KeyAttribute : Attribute
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
    [Obsolete("改属性弃用，改用IgnoreWriteAttribute")]
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
    #endregion
}

#region -- SqlAdapter
/// <summary>
/// 
/// </summary>
public partial interface ISqlAdapter
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

public partial class SqlServerAdapter : ISqlAdapter
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

public partial class SqlCeServerAdapter : ISqlAdapter
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

public partial class MySqlAdapter : ISqlAdapter
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