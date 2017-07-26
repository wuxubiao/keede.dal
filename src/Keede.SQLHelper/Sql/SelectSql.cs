using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class SelectSql : SqlBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        public SelectSql(string tableName)
            : base(tableName, ExecuteSqlType.Select)
        {
            JoinTables = new StringBuilder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static SelectSql FromTable(string tableName)
        {
            return new SelectSql(tableName);
        }

        #region -- Field()

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public SelectSql SelectFields(params string[] fields)
        {
            Fields = fields.ToList();
            return this;
        }

        #endregion

        #region -- Where()

        /// <summary>
        /// Where > 筛选
        /// </summary>
        /// <param name="fields">
        /// <para>格式写法：[FieldName=ParameterName] OR [FieldName]</para>
        /// <para>
        /// <example>
        /// Example：SetWhere(new string[]{"Nick=@Nick","Sex=@Sex","Age=22"},new object[]{nick,sex})
        /// </example>
        /// </para>
        /// </param>
        /// <param name="values"></param>
        public SelectSql Where(string[] fields, params object[] values)
        {
            WhereFields = ReproduceParameterNames(fields).ToArray();
            WhereValues = values;
            var parameters = fields.ToList().Select(n =>
            {
                if (regexParameter.IsMatch(n))
                {
                    return regexParameter.Match(n).Groups[0].Value;
                }
                return string.Empty;
            }).ToArray();
            AddParameters(parameters, values);
            return this;
        }

        /// <summary>
        /// 筛选，默认都是AND条件并列
        /// </summary>
        /// <param name="fieldExpression">
        ///     <para>格式写法：[FieldName=ParameterName] OR [FieldName]</para>
        ///     <para>
        ///         <example>
        ///             Example：SetWhere(new string[]{"Nick=@Nick","Sex=@Sex","Age=22"},new object[]{nick,sex})
        ///         </example>
        ///     </para>
        /// </param>
        /// <param name="values"></param>
        public SelectSql Where(string fieldExpression, params object[] values)
        {
            WhereFields = fieldExpression.Split(',');
            WhereValues = values;
            var parameters = WhereFields.ToList().Select(n =>
            {
                if (regexParameter.IsMatch(n))
                {
                    return regexParameter.Match(n).Groups[0].Value;
                }
                return string.Empty;
            }).ToArray();
            AddParameters(parameters, values);
            return this;
        }

        #endregion

        #region -- OrderBy()
        internal string OrderByFields { get; set; }
        /// <summary>
        /// 排序字段，可以复写多个
        /// </summary>
        /// <param name="orderByFields">
        /// <para>排序字段</para>
        /// <example>参数格式：Age DESC,Birthday ASC</example>
        /// </param>
        /// <returns></returns>
        public SelectSql OrderBy(string orderByFields)
        {
            OrderByFields = orderByFields;
            return this;
        }

        #endregion

        #region -- Join()
        internal class JoinTable
        {
            internal JoinTable(JoinType type,string table, string onCompare)
            {
                JoinType = type;
                TableName = table;
                OnCompare = onCompare;
            }
            internal JoinType JoinType { get; private set; }
            internal string TableName { get; private set; }
            internal string OnCompare { get; private set; }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (JoinType == JoinType.InnerJoin)
                {
                    sb.AppendLine(" INNER JOIN ");
                }
                else if (JoinType == JoinType.LeftJoin)
                {
                    sb.AppendLine(" LEFT JOIN ");
                }
                else if (JoinType == JoinType.RightJoin)
                {
                    sb.AppendLine(" RIGHT JOIN ");
                }
                sb.Append(TableName + " ON " + OnCompare);
                return sb.ToString();
            }
            internal static JoinTable Create(JoinType type, string table, string onCompare)
            {
                return new JoinTable(type, table, onCompare);
            }
        }
        internal StringBuilder JoinTables { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableOrAs"></param>
        /// <param name="onCompare"></param>
        /// <returns></returns>
        public SelectSql Join(JoinType type, string tableOrAs, string onCompare)
        {
            JoinTables.AppendLine(JoinTable.Create(type, tableOrAs, onCompare).ToString());
            return this;
        }

        #endregion

        #region -- ToString()
        /// <summary>
        /// 生成脚本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            sqlBuilder.Append(SQL_SELECT);
            sqlBuilder.Append(string.Join(",", Fields.ToArray()));
            sqlBuilder.Append(SQL_FROM + TableName);
            sqlBuilder.AppendLine(JoinTables.ToString());
            if (WhereFields != null)
            {
                sqlBuilder.Append(SQL_WHERE);
                sqlBuilder.Append(string.Join(SQL_AND, WhereFields));
            }
            if (OrderByFields != null)
            {
                sqlBuilder.AppendLine(" ORDER BY " + OrderByFields);
            }
            return sqlBuilder.ToString();
        }
        #endregion
    }
}
