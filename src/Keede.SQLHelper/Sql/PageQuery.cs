using System;
using System.Linq;

namespace Keede.SQLHelper.Sql
{
    /// <summary>
    ///
    /// </summary>
    public class PageQuery : QueryBase
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="selectQuery">查询脚本</param>
        /// <param name="startPage">当前第几页</param>
        /// <param name="limitRow">限制读取行数</param>
        /// <param name="orderByFields">分页查询的降序或升序，如果不指明取Select语句内的第一个非“*”字段，例如：fieldName DESC</param>
        public PageQuery(int startPage, int limitRow, string selectQuery, params string[] orderByFields)
            : base(selectQuery)
        {
            OrderByFields = orderByFields.ToList();
            if (startPage < 1)
            {
                StartRow = 1;
            }
            else
            {
                StartRow = (startPage - 1) * limitRow + 1;
            }
            EndRow = StartRow + (limitRow - 1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="selectQuery">查询脚本</param>
        /// <param name="startRow">当前第几行</param>
        /// <param name="limitRow">限制读取行数</param>
        /// <param name="orderByFields">分页查询的降序或升序，如果不指明取Select语句内的第一个非“*”字段，例如：fieldName DESC</param>
        public PageQuery(long startRow, int limitRow, string selectQuery, params string[] orderByFields)
            : base(selectQuery)
        {
            OrderByFields = orderByFields.ToList();
            if (startRow < 1)
            {
                StartRow = 1;
            }
            else
            {
                StartRow = startRow;
            }
            EndRow = StartRow + (limitRow - 1);
        }

        /// <summary>
        ///
        /// </summary>
        public long StartRow { get; set; }

        /// <summary>
        ///
        /// </summary>
        public long EndRow { get; set; }

        #region -- ToCounyQuery()

        /// <summary>
        /// 生成统计数量的脚本
        /// </summary>
        /// <returns></returns>
        public string ToCountQuery()
        {
            return "SELECT COUNT(0) FROM ( " + SelectQuery + " ) tt";
        }

        #endregion -- ToCounyQuery()

        #region -- ToString()

        /// <summary>
        /// 生成带有分页的全部脚本
        /// </summary>
        /// <returns></returns>
        public string ToFullQuery()
        {
            var query = PageQueryString.Replace("{select_query}", SelectQuery);
            if (OrderByFields.Count == 0)
            {
                var fields = Fields;
                if (fields[0] == "*")
                {
                    throw new ArgumentNullException("","没有指明分页查询的Over(Order By ...)内的排序字段");
                }
                query = query.Replace("{field_asc_desc}", fields[0]);
            }
            else
            {
                query = query.Replace("{field_asc_desc}", string.Join(",", OrderByFields.ToArray()));
            }
            query = query.Replace("{start}", StartRow.ToString());
            query = query.Replace("{end}", EndRow.ToString());
            return query;
        }

        #endregion -- ToString()
    }
}