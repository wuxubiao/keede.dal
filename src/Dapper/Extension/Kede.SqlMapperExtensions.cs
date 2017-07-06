using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    public static partial class SqlMapperExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereSql"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static bool Delete<T>(this IDbConnection connection, string whereSql, object parameterObject = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {

            whereSql = whereSql.Trim();
            if (string.IsNullOrEmpty(whereSql)) throw new ArgumentNullException("whereSql is null");

            whereSql = whereSql.StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + whereSql + " ";
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = $"delete from {name}" + whereSql;

            var deleted = connection.Execute(statement, parameterObject, transaction, commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id or number if inserted rows if inserting a list.
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
                return p.GetCustomAttributes(true).Any(a => a is AutoKeyAttribute);
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
            return connection.Execute(sql, dynParms, transaction, commandTimeout) > 0;
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
        public static long InsertBulk<T>(this IDbConnection connection, IList<T> list, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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
    }
}