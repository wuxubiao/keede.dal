using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace Framework.Core.DB.DapperExtension
{

    /// <summary>
    /// 
    /// </summary>
    public static partial class SqlMapperExtensions
    {
        /// <summary>Query paged data from a single table.
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
            string columns = $"[{string.Join("],[", canReadProperties.Select(p => p.Name).ToArray())}]";
            var table = GetTableName(type);
            var sql = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER ({1}) AS RowNumber, {0} FROM {2}{3}) AS Total WHERE RowNumber >= {4} AND RowNumber <= {5}", columns, pagedList.OrderBy, table, pagedList.WhereSql, (pagedList.PageIndex - 1) * pagedList.PageSize + 1, pagedList.PageIndex * pagedList.PageSize);
            var datas = connection.Query<T>(sql, paramterObjects, transaction, true, commandTimeout).ToList();
            var countSql = $"SELECT COUNT(0) FROM {table} {pagedList.WhereSql} ";
            var total = connection.QueryFirstOrDefault<int>(countSql, paramterObjects, transaction);
            pagedList.FillQueryData(total, datas);
        }
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
            WhereSql = whereSql.Trim().StartsWith("WHERE", StringComparison.CurrentCultureIgnoreCase) ? " " + whereSql + " " : " WHERE " + WhereSql + " ";
            OrderBy = orderBy.Trim().StartsWith("ORDER BY",StringComparison.CurrentCultureIgnoreCase)?" "+orderBy+" ":" ORDER BY "+orderBy+" ";
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
        public long PageSize { get; }

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
