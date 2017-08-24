#if ASYNC
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
    public static partial class SqlMapperExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return await GetAsync<T>(connection, id, false, transaction, commandTimeout);
        }

        /// <summary>
        /// 有改动，select按IsReadable取出
        /// 阮哥
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <param name="isUpdateLock"></param>
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
        public static async Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, bool isUpdateLock, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var key = GetSingleKey<T>(nameof(Get));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => GetCustomColumnName(p)).ToArray())}]";
                if (isUpdateLock)
                {
                    sql = $"select {columns} from {name} WITH (UPDLOCK) where {GetCustomColumnName(key)} = @id";
                }
                else
                {
                    sql = $"select {columns} from {name} where {GetCustomColumnName(key)} = @id";
                }
                GetQueries[type.TypeHandle] = sql;
            }

            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);

            T obj;

            if (type.IsInterface())
            {
                var res = await connection.QueryAsync(sql, dynParms).Result.FirstOrDefault() as IDictionary<string, object>;

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
                obj = connection.QueryAsync<T>(sql, dynParms, transaction, commandTimeout: commandTimeout).Result.FirstOrDefault();
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
        public static async Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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

            if (!type.IsInterface()) return await connection.QueryAsync<T>(sql, null, transaction, commandTimeout);

            var result = await connection.QueryAsync(sql);
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
        /// Inserts an entity into table "Ts" asynchronously using .NET 4.5 Task and returns identity id.
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <param name="sqlAdapter">The specific ISqlAdapter to use, auto-detected based on connection if null</param>
        /// <returns>Identity of inserted entity</returns>
        public static async Task<int> InsertAsync<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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
                returnVal = await adapter.InsertAsync(connection, transaction, commandTimeout, name, sbColumnList.ToString(),
                    sbParameterList.ToString(), keyProperties, entityToInsert);
            }
            else                        //非自增列和列表
            {
                //insert list of entities
                var cmd = $"insert into {name} ({sbColumnList}) values ({sbParameterList})";
                returnVal = await connection.ExecuteAsync(cmd, entityToInsert, transaction, commandTimeout);
            }
            if (wasClosed) connection.Close();
            return returnVal;
        }

        /// <summary>
        /// Updates entity in table "Ts" asynchronously using .NET 4.5 Task, checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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
            var updated = await connection.ExecuteAsync(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout, transaction: transaction);
            return updated > 0;
        }

        /// <summary>
        /// Delete entity in table "Ts" asynchronously using .NET 4.5 Task.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if not found</returns>
        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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
            var deleted = await connection.ExecuteAsync(sb.ToString(), entityToDelete, transaction, commandTimeout);
            return deleted > 0;
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
        public static Task<int> DeleteAsync<T>(this IDbConnection connection, string whereSql, object parameterObject = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {

            whereSql = whereSql.Trim();
            if (string.IsNullOrEmpty(whereSql)) throw new ArgumentNullException("whereSql is null");

            whereSql = whereSql.StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + whereSql + " ";
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = $"delete from {name}" + whereSql;

            return connection.ExecuteAsync(statement, parameterObject, transaction, commandTimeout);
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
        public static async Task<PagedList<T>> QueryPagedAsync<T>(this IDbConnection connection, PagedList<T> pagedList, object paramterObjects = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var type = typeof(T);
            var canReadProperties = TypePropertiesCanReadCache(type);
            if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
            string columns = $"[{string.Join("],[", canReadProperties.Select(p => GetCustomColumnName(p)).ToArray())}]";
            var table = GetTableName(type);
            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER ({1}) AS RowNumber, {0} FROM {2}{3}) AS Total WHERE RowNumber >= {4} AND RowNumber <= {5}", columns, pagedList.OrderBy, table, pagedList.WhereSql, (pagedList.PageIndex - 1) * pagedList.PageSize + 1, pagedList.PageIndex * pagedList.PageSize);
            var datas = connection.QueryAsync<T>(sql, paramterObjects, transaction, commandTimeout).Result.ToList();
            var countSql = $"SELECT COUNT(0) FROM {table} {pagedList.WhereSql} ";
            var total =  connection.QueryFirstOrDefaultAsync<int>(countSql, paramterObjects, transaction).Result;
            pagedList.FillQueryData(total, datas);
            return pagedList;
        }

        /// <summary>
        /// 目前只有针对sql server的实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="paramterObjects"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<List<T>> QueryPagedAsync<T>(this IDbConnection connection, string sql, int pageIndex, int pageSize, object paramterObjects = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var commandText = ProcessCommandSqlServer(sql.Trim(), pageIndex, pageSize);

            return  connection.QueryAsync<T>(commandText, paramterObjects, transaction, commandTimeout).Result.ToList();
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
        public static async Task<IEnumerable<T>> QueryListAsync<T>(this IDbConnection connection, object condition, string table, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return await connection.QueryAsync<T>(BuildQuerySQL(condition, table, columns, isOr), condition, transaction, commandTimeout);
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
        public static async Task<PagedList<T>> QueryPagedAsync<T>(this IDbConnection connection, dynamic condition, string table, string orderBy, int pageIndex, int pageSize, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
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

            var datas =  await connection.QueryAsync<T>(sql, conditionObj, transaction, commandTimeout);
            var list = datas.ToList();
            var total = GetCount(connection, condition, table, isOr, transaction, commandTimeout);
            return new PagedList<T>(pageIndex, pageSize, total, list);
        }
    }
}
#endif