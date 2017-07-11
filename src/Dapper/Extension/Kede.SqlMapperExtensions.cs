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
            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var key = GetSingleKey<T>(nameof(Get));
                var name = GetTableName(type);
                var canReadProperties = TypePropertiesCanReadCache(type);
                if (canReadProperties.Count == 0) throw new ArgumentException("Entity must have at least one property for Select");
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
                sql = $"select {columns} from {name} where {key.Name} = @id";
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
                string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
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
        #endregion 改动方法
        
        #region 新增方法
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
        public static long InsertEx<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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
            string sql = $"update {name} set {key.Name}=@id where {key.Name} = @id";
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
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static long BatchInsert<T>(this IDbConnection connection, IList<T> list, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            long deleted = 0;
            //if (list != null && list.Count > 0)
            //{
            //    T model = list.FirstOrDefault();
            //    var ps = model.GetType().GetProperties();
            //    List<string> @colms = new List<string>();
            //    List<string> @params = new List<string>();

            //    foreach (var p in ps)
            //    {
            //        if (!p.CustomAttributes.Any(x => x.AttributeType == typeof(PrimaryKeyAttribute)) && !p.CustomAttributes.Any(x => x.AttributeType == typeof(DBIgnoreAttribute)))
            //        {
            //            @colms.Add(string.Format("[{0}]", p.Name));
            //            @params.Add(string.Format("@{0}", p.Name));
            //        }
            //    }
            //    var sql = string.Format("INSERT INTO [{0}] ({1}) VALUES({2})", typeof(T).Name, string.Join(", ", @colms), string.Join(", ", @params));

            //    deleted = connection.Execute(sql, list, null, null, null);
            //}

            return deleted;
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

            foreach (var propertie in allPropertiesExceptKeyAndComputed)
            {
                var column = new DataColumn(propertie.Name, propertie.PropertyType);
                dt.Columns.Add(column);
            }

            foreach (var item in list)
            {
                DataRow rs = dt.NewRow();
                int i = 0;
                foreach(var propertie in allPropertiesExceptKeyAndComputed)
                {
                    rs[i] = type.GetProperty(propertie.Name).GetValue(item, null);
                    i++;
                }

                dt.Rows.Add(rs);
            }

            return dt;
        }

        #endregion 新增方法
    }

    //#region 新增Attribute
    ///// <summary>
    ///// 
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Property)]
    //public class IgnoreReadAttribute : Attribute
    //{
    //}
    //#endregion 新增Attribute
}