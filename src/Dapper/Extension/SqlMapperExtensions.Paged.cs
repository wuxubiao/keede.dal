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
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="paramterObjects"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IList<T> QueryPaged<T>(this IDbConnection connection, string sql, int pageIndex, int pageSize, string orderBy, object paramterObjects = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var commandText = ProcessCommandSqlServer(sql.Trim(), pageIndex, pageSize, orderBy);

            IList<T> result = connection.Query<T>(commandText, paramterObjects, transaction, true, commandTimeout).ToList();
            return result;
        }

        /// <summary>
        /// order by 正则
        /// </summary>
        private static readonly Regex _orderByRegexSqlServer = new Regex(@"\s*order\s+by\s+[^\s,\)\(]+(?:\s+(?:asc|desc))?(?:\s*,\s*[^\s,\)\(]+(?:\s+(?:asc|desc))?)*", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RxOrderBy = new Regex(@"\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex RxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        //private static readonly string _tableAndWhere = @"[Ff][Rr][Oo][Mm][\s\S]+[Ww][Hh][Ee][Rr][Ee][\s\S]+";

        /// <summary>
        /// 获取最后一个匹配的 Order By 结果。
        /// </summary>
        /// <param name="commandText">原查询字符串。</param>
        /// <returns>返回 Order By 结果。</returns>
        private static Match GetOrderByMatch(string commandText)
        {
            //var match = _orderByRegexSqlServer.Match(commandText);
            var match = RxOrderBy.Match(commandText);
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
        /// <param name="orderBy"></param>
        private static string ProcessCommandSqlServer(string commandText, int pageIndex, int pageSize, string orderBy)
        {
            var start = pageSize * (pageIndex - 1) + 1;
            var end = pageIndex * pageSize;

            if (string.IsNullOrEmpty(orderBy))
            {
                var match = GetOrderByMatch(commandText);
                if (match.Success)
                {
                    commandText = commandText.Remove(match.Index);
                    orderBy = match.Value.Trim();
                }
                else
                {
                    orderBy = "ORDER BY getdate()";
                }
            }
            else
            {
                if (!orderBy.Trim().ToUpper().StartsWith("ORDER BY"))
                {
                    orderBy = "ORDER BY " + orderBy;
                }
            }

            var m = RxColumns.Match(commandText);
            if (!m.Success)
                throw new ArgumentException();

            Group g = m.Groups[1];
            var sqlSelectRemoved = commandText.Substring(g.Index);

            return string.Format(@"SELECT * FROM (SELECT ROW_NUMBER() OVER({4}) AS {1},{0}) ____t1____ WHERE {1} BETWEEN {2} AND {3}",
                sqlSelectRemoved
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
            if (!string.IsNullOrEmpty(whereSql))
                WhereSql = whereSql.Trim().StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + whereSql + " ";
            OrderBy = orderBy.Trim().StartsWith("ORDER BY", StringComparison.CurrentCultureIgnoreCase) ? " " + orderBy + " " : " ORDER BY " + orderBy + " ";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordCount"></param>
        /// <param name="dataList"></param>
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
