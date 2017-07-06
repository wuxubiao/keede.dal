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
            if (string.IsNullOrEmpty(whereSql)) throw new ArgumentNullException("where is null");

            whereSql = whereSql.StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + whereSql + " ";
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = $"delete from {name}" + whereSql;

            var deleted = connection.Execute(statement, parameterObject, transaction, commandTimeout);
            return deleted > 0;
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
