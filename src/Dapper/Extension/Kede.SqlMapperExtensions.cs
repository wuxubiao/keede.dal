/*
 * 
 * 
 * 
 * 
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dapper.Extension
{
    public static partial class SqlMapperExtensions
    {
        #region 改动方法
        /// <summary>
        /// ！@#有改动
        /// 阮哥
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
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
        /// ！@#有改动，select按IsReadable取出
        /// 阮哥
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
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            //var allProperties = TypePropertiesCache(type);
            //var keyProperties = KeyPropertiesCache(type);
            //var computedProperties = ComputedPropertiesCache(type);
            //var allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties)).ToList();


            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var key = GetSingleKey<T>(nameof(Get));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => GetCustomColumnName(p)).ToArray())}]";
                sql = $"select {columns} from {name} where {GetCustomColumnName(key)} = @id";
                GetQueries[type.TypeHandle] = sql;
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

        /// <summary>Query paged data from a single table.
        /// 目前只有针对sql server的实现
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="pagedList"></param>
        /// <param name="paramterObjects"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout">超时时间，单位：秒</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void QueryPaged<T>(this IDbConnection connection, ref PagedList<T> pagedList, object paramterObjects = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var type = typeof(T);
            var canReadProperties = TypePropertiesCanReadCache(type);
            if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
            string columns = $"[{string.Join("],[", canReadProperties.Select(p => GetCustomColumnName(p)).ToArray())}]";
            var table = GetTableName(type);
            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER ({1}) AS RowNumber, {0} FROM {2}{3}) AS Total WHERE RowNumber >= {4} AND RowNumber <= {5}", columns, pagedList.OrderBy, table, pagedList.WhereSql, (pagedList.PageIndex - 1) * pagedList.PageSize + 1, pagedList.PageIndex * pagedList.PageSize);
            var datas = connection.Query<T>(sql, paramterObjects, transaction, true, commandTimeout).ToList();
            var countSql = $"SELECT COUNT(0) FROM {table} {pagedList.WhereSql} ";
            var total = connection.QueryFirstOrDefault<int>(countSql, paramterObjects, transaction);
            pagedList.FillQueryData(total, datas);
        }

        /// <summary>
        /// ！@#有改动，select按IsReadable取出
        /// 阮哥
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
            if (!GetQueries.TryGetValue(cacheType.TypeHandle, out sql))
            {
                //GetSingleKey<T>(nameof(GetAll));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => GetCustomColumnName(p)).ToArray())}]";
                sql = $"SELECT {columns} FROM " + name;
                GetQueries[cacheType.TypeHandle] = sql;
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
                AppendColumnNameEqualsValue(sb, property);  //fix for issue #336
                if (i < nonIdProps.Count - 1)
                    sb.AppendFormat(", ");
            }
            sb.Append(" where ");
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties.ElementAt(i);
                AppendColumnNameEqualsValue(sb, property);  //fix for issue #336
                if (i < keyProperties.Count - 1)
                    sb.AppendFormat(" and ");
            }
            var updated = connection.Execute(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout, transaction: transaction);
            return updated > 0;
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
                AppendColumnNameEqualsValue(sb, property);  //fix for issue #336
                if (i < keyProperties.Count - 1)
                    sb.AppendFormat(" and ");
            }
            var deleted = connection.Execute(sb.ToString(), entityToDelete, transaction, commandTimeout);
            return deleted > 0;
        }

        #endregion 改动方法

        #region 新增方法
        private static void AppendColumnNameEqualsValue(StringBuilder sb, PropertyInfo property)
        {
            sb.AppendFormat("[{0}] = @{1}", GetCustomColumnName(property), property.Name);
        }

        private static string GetCustomColumnName(PropertyInfo property)
        {
            string result = property.Name;
            var attributes = property.GetCustomAttributes(true).Where(p => p is ColumnAttribute).FirstOrDefault();
            if (attributes != null)
            {
                result = (attributes as ColumnAttribute).Name;
            }
            return result;
        }

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _readTypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
            if (!GetQueries.TryGetValue(cacheType.TypeHandle, out sql))
            {
                //GetSingleKey<T>(nameof(GetAll));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
                if (!whereSql.StartsWith("where"))
                {
                    whereSql = " WHERE " + whereSql;
                }
                sql = $"SELECT {columns} FROM " + name + whereSql + orderBy;
                GetQueries[cacheType.TypeHandle] = sql;
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
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection connection, string whereSql, object parameterObject = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {

            whereSql = whereSql.Trim();
            if (string.IsNullOrEmpty(whereSql)) throw new ArgumentNullException("whereSql is null");

            whereSql = whereSql.StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + whereSql + " ";
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = $"delete from {name}" + whereSql;

            return connection.Execute(statement, parameterObject, transaction, commandTimeout);
        }

        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id or number if inserted rows if inserting a list.
        /// 
        /// 修改SqlMapperExtensions
        /// long Insert<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        /// 修复了主键为非自增列的表不能返回正确结果
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert, can be list of entities</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <code>
        /// <example>
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

            var isAutoKey = allProperties.Where(p =>
            {
                return p.GetCustomAttributes(true).Any(a => a is KeyAttribute);
            }).Count() > 0;

            var adapter = GetFormatter(connection);

            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count; i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);

                adapter.AppendColumnName(sbColumnList, GetCustomColumnName(property));  //fix for issue #336
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

            if (!isList && isAutoKey)    //主键为自增列
            {
                returnVal = adapter.Insert(connection, transaction, commandTimeout, name, sbColumnList.ToString(),
                    sbParameterList.ToString(), keyProperties, entityToInsert);
            }
            else                        //非自增列和列表
            {
                //insert list of entities
                var cmd = $"insert into {name} ({sbColumnList}) values ({sbParameterList})";
                returnVal = connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            }
            if (wasClosed) connection.Close();
            return returnVal;
        }

        /// <summary>
        /// 性能有问题，锁表操作
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
            string sql = $"update {name} set {GetCustomColumnName(key)}=@id where {GetCustomColumnName(key)} = @id";
            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);
            return connection.Execute(sql, dynParms, transaction, commandTimeout) > 0;
        }

        private static IList<PropertyInfo> GetKeys<T>(string method)
        {
            var type = typeof(T);
            var keys = KeyPropertiesCache(type);
            var explicitKeys = ExplicitKeyPropertiesCache(type);
            var keyCount = keys.Count + explicitKeys.Count;
            //if (keyCount > 1)
            //    throw new DataException($"{method}<T> only supports an entity with a single [Key] or [ExplicitKey] property");
            if (keyCount == 0)
                throw new DataException($"{method}<T> only supports an entity with a [Key] or an [ExplicitKey] property");

            return keys.Any() ? keys : explicitKeys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable GetTableSchema<T>(this IDbConnection connection, IList<T> list)
        {
            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                type = type.GetGenericArguments()[0];
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = KeyPropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);
            var allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties)).ToList();


            DataTable dt = new DataTable();
            dt.TableName = GetTableName(type);

            if (keyProperties.Count > 0)
            {
                dt.Columns.Add(new DataColumn(GetCustomColumnName(keyProperties[0]), keyProperties[0].PropertyType));
            }

            foreach (var propertie in allPropertiesExceptKeyAndComputed)
            {
                var column = new DataColumn(GetCustomColumnName(propertie), propertie.PropertyType);
                dt.Columns.Add(column);
            }

            foreach (var item in list)
            {
                DataRow rs = dt.NewRow();
                int i = keyProperties.Count > 0 ? 1 : 0;
                foreach (var propertie in allPropertiesExceptKeyAndComputed)
                {
                    rs[i] = type.GetProperty(propertie.Name).GetValue(item, null);
                    i++;
                }

                dt.Rows.Add(rs);
            }

            return dt;
        }

        #region SqlMapper_Extensions
        /// <summary>Get data count from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int GetCount(this IDbConnection connection, object condition, string table, bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<int>(connection, condition, table, "count(*)", isOr, transaction, commandTimeout).Single();
        }

        /// <summary>Query a list of data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryList(this IDbConnection connection, dynamic condition, string table, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<dynamic>(connection, condition, table, columns, isOr, transaction, commandTimeout);
        }

        /// <summary>Query a list of data from table with specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> QueryList<T>(this IDbConnection connection, object condition, string table, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query<T>(BuildQuerySQL(condition, table, columns, isOr), condition, transaction, true, commandTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static PagedList<dynamic> QueryPaged(this IDbConnection connection, dynamic condition, string table, string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryPaged<dynamic>(connection, condition, table, orderBy, pageIndex, pageSize, columns, isOr, transaction, commandTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static PagedList<T> QueryPaged<T>(this IDbConnection connection, dynamic condition, string table, string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var properties = GetFieldNames(conditionObj);
            if (properties.Count > 0)
            {
                var separator = isOr ? " OR " : " AND ";
                whereFields = " WHERE " + string.Join(separator, properties.Select(p => HandleKeyword(p) + " = @" + p));
            }
            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RowNumber, {0} FROM {2}{3}) AS Total WHERE RowNumber >= {4} AND RowNumber <= {5}", columns, orderBy, table, whereFields, (pageIndex - 1) * pageSize + 1, pageIndex * pageSize);

            var datas = connection.Query<T>(sql, conditionObj, transaction, true, commandTimeout).ToList();
            var total = GetCount(connection, condition, table, isOr, transaction, commandTimeout);
            return new PagedList<T>(pageIndex, pageSize, total, datas);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert(this IDbConnection connection, dynamic data, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var properties = GetPropertyAndFieldName(obj);
            var fieldNames = properties.Values.ToList();
            var columns = string.Join(",", fieldNames);
            var values = string.Join(",", fieldNames.Select(p => "@" + p));
            var sql = string.Format("insert into [{0}] ({1}) values ({2}) select cast(scope_identity() as bigint)", table, columns, values);

            //return connection.Execute(sql, obj, transaction, commandTimeout);

            var parameters = new DynamicParameters(data);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            //properties.ForEach(p => expandoObject.Add("@" + p.Value, p.Key.GetValue(data, null)));
            foreach (var item in properties)
            {
                expandoObject.Add("@" + item.Value, item.Key.GetValue(data, null));
            }

            parameters.AddDynamicParams(expandoObject);

            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update(this IDbConnection connection, dynamic data, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var conditionObj = condition as object;

            var wherePropertyInfos = GetPropertyAndFieldName(conditionObj);

            var updateProperties = GetFieldNames(obj);
            var whereProperties = wherePropertyInfos.Values.ToList();

            var updateFields = string.Join(",", updateProperties.Select(p => HandleKeyword(p) + " = @" + p));
            var whereFields = string.Empty;

            if (whereProperties.Any())
            {
                whereFields = " where " + string.Join(" and ", whereProperties.Select(p => p + " = @w_" + p));
            }

            var sql = string.Format("update [{0}] set {1}{2}", table, updateFields, whereFields);

            var parameters = new DynamicParameters(data);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            //wherePropertyInfos.ForEach(p => expandoObject.Add("w_" + p.Value, p.Key.GetValue(conditionObj, null)));

            foreach (var item in wherePropertyInfos)
            {
                expandoObject.Add("w_" + item.Value, item.Key.GetValue(conditionObj, null));
            }

            parameters.AddDynamicParams(expandoObject);

            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var whereProperties = GetFieldNames(conditionObj);
            if (whereProperties.Count > 0)
            {
                whereFields = " where " + string.Join(" and ", whereProperties.Select(p => HandleKeyword(p) + " = @" + p));
            }

            var sql = string.Format("delete from [{0}]{1}", table, whereFields);

            return connection.Execute(sql, conditionObj, transaction, commandTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="selectPart"></param>
        /// <param name="isOr"></param>
        /// <returns></returns>
        private static string BuildQuerySQL(dynamic condition, string table, string selectPart = "*", bool isOr = false)
        {
            var conditionObj = condition as object;
            var properties = GetFieldNames(conditionObj);
            if (properties.Count == 0)
            {
                return string.Format("SELECT {1} FROM [{0}]", table, selectPart);
            }

            var separator = isOr ? " OR " : " AND ";
            var wherePart = string.Join(separator, properties.Select(p => HandleKeyword(p) + " = @" + p));

            return string.Format("SELECT {2} FROM [{0}] WHERE {1}", table, wherePart, selectPart);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static List<string> GetFieldNames(object obj)
        {
            if (obj == null)
            {
                return new List<string>();
            }
            if (obj is DynamicParameters)
            {
                return (obj as DynamicParameters).ParameterNames.ToList();
            }
            Dictionary<PropertyInfo, string> propertyAndField = GetPropertyAndFieldName(obj);
            return propertyAndField.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string HandleKeyword(string p)
        {
            switch (p)
            {
                case "Key":
                    return "[Key]";
                case "Description":
                    return "[Description]";
                default:
                    return p;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, string>> _paramCache = new ConcurrentDictionary<Type, Dictionary<PropertyInfo, string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static Dictionary<PropertyInfo, string> GetPropertyAndFieldName(object obj)
        {
            if (obj == null)
            {
                return new Dictionary<PropertyInfo, string>();
            }

            Dictionary<PropertyInfo, string> propertyAndField;
            if (_paramCache.TryGetValue(obj.GetType(), out propertyAndField))
                return propertyAndField;

            propertyAndField = new Dictionary<PropertyInfo, string>();
            var properties = obj.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public).ToList();

            foreach (var property in properties)
            {
                var computedAttribute = property.GetCustomAttributes(typeof(ComputedAttribute), false).SingleOrDefault();
                if (computedAttribute != null)
                    continue;

                var attribute = property.GetCustomAttributes(typeof(ColumnAttribute), false).SingleOrDefault();

                if (attribute == null)
                {
                    propertyAndField.Add(property, property.Name);
                }
                else
                {
                    var columnAttribute = (ColumnAttribute)attribute;

                    propertyAndField.Add(property, string.IsNullOrWhiteSpace(columnAttribute.Name)
                        ? property.Name
                        : columnAttribute.Name);
                }
            }

            _paramCache[obj.GetType()] = propertyAndField;
            return propertyAndField;
        }
        #endregion SqlMapper_Extensions

        #endregion 新增方法
    }
}