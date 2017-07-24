using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Text.RegularExpressions;

namespace Dapper.Extension
{

    /// <summary>
    /// 
    /// </summary>
    public static partial class SqlMapperExtensions
    {


        #region 新增分页方法
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
        public static List<T> QueryPaged<T>(this IDbConnection connection, string sql, int pageIndex, int pageSize, object paramterObjects = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var commandText = ProcessCommandSqlServer(sql.Trim(),pageIndex,pageSize);

            return connection.Query<T>(commandText, paramterObjects, transaction, true, commandTimeout).ToList();
        }

        private static readonly Regex OrderByRegexSqlServer = new Regex(@"\s*order\s+by\s+[^\s,\)\(]+(?:\s+(?:asc|desc))?(?:\s*,\s*[^\s,\)\(]+(?:\s+(?:asc|desc))?)*", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 获取最后一个匹配的 Order By 结果。
        /// </summary>
        /// <param name="commandText">原查询字符串。</param>
        /// <returns>返回 Order By 结果。</returns>
        private static Match GetOrderByMatch(string commandText)
        {
            var match = OrderByRegexSqlServer.Match(commandText);
            while (match.Success)
            {
                if ((match.Index + match.Length) == commandText.Length) return match;
                match = match.NextMatch();
            }
            return match;
        }

        /// <summary>
        /// 创建指定查询字符串的统计总行数查询字符串。
        /// </summary>
        /// <param name="pageIndex">从 1 开始的页码。</param>
        /// <param name="pageSize">页的大小。</param>
        /// <param name="commandText">数据源查询命令。</param>
        private static string ProcessCommandSqlServer(string commandText,int pageIndex, int pageSize)
        {
            var start = (pageIndex - 1) * pageSize;
            var end = pageIndex * pageSize;
            var match = GetOrderByMatch(commandText);
            var orderBy = "ORDER BY getdate()";
            if (match.Success)
            {
                commandText = commandText.Remove(match.Index);
                orderBy = match.Value.Trim();
            }

            return string.Format(@"SELECT * FROM (SELECT ROW_NUMBER() OVER({4}) AS {1},* FROM ({0}) ____t1____) ____t2____ WHERE {1}>{2} AND {1}<={3}", 
                commandText
                , "RowNumber"
                , start
                , end
                , orderBy);
        }
        #endregion 新增分页方法
    }

    /// <summary>
    /// 分页对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T>
    {
        ///  <summary>
        /// 
        ///  </summary>
        ///  <param name="pageIndex"></param>
        ///  <param name="pageSize"></param>
        ///  <param name="whereSql"></param>
        /// <param name="orderBy"></param>
        public PagedList(int pageIndex, int pageSize, string whereSql = "", string orderBy = "")
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            WhereSql = whereSql.Trim().StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + whereSql + " ";
            OrderBy = orderBy.Trim().StartsWith("ORDER BY",StringComparison.CurrentCultureIgnoreCase)?" "+orderBy+" ":" ORDER BY "+orderBy+" ";
        }

        public PagedList(int pageIndex, int pageSize, long recordCount, IList<T> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            RecordCount = recordCount;
            Items = items;
            PageCount = new Func<long>(delegate
            {
                var pages = RecordCount / PageSize;
                if (RecordCount % PageSize != 0)
                {
                    pages = pages + 1;
                }
                if (PageIndex > pages)
                {
                    PageIndex = pages;
                }
                return pages;
            }).Invoke();
        }

        internal void FillQueryData(int recordCount, IList<T> dataList)
        {
            RecordCount = recordCount;
            Items = dataList;
            PageCount = new Func<long>(delegate
            {
                var pages = RecordCount / PageSize;
                if (RecordCount % PageSize != 0)
                {
                    pages = pages + 1;
                }
                if (PageIndex > pages)
                {
                    PageIndex = pages;
                }
                return pages;
            }).Invoke();
        }

        /// <summary>
        ///
        /// </summary>
        public long PageIndex { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long PageSize { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long PageCount { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long RecordCount { get; private set; }

        /// <summary>
        /// 筛选条件
        /// </summary>
        public string WhereSql { get; }
        /// <summary>
        /// 排序条件
        /// </summary>
        public string OrderBy { get; }

        /// <summary>
        ///
        /// </summary>
        public IList<T> Items { get; private set; }
    }
}
