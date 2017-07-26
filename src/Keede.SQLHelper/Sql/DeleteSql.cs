using System.Linq;

namespace Framework.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteSql : SqlBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        public DeleteSql(string tableName)
            : base(tableName, ExecuteSqlType.Update)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DeleteSql FromTable(string tableName)
        {
            return new DeleteSql(tableName);
        }

        /// <summary>
        /// 筛选，默认都是AND条件并列
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
        public DeleteSql Where(string[] fields, params object[] values)
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
        /// Where > 筛选
        /// </summary>
        /// <param name="fieldExpression">
        /// <para>格式写法：[FieldName=ParameterName] OR [FieldName]</para>
        /// <para>
        /// <example>
        /// Example：SetWhere(new string[]{"Nick=@Nick","Sex=@Sex","Age=22"},new object[]{nick,sex})
        /// </example>
        /// </para>
        /// </param>
        /// <param name="values"></param>
        public DeleteSql Where(string fieldExpression, params object[] values)
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

        /// <summary>
        /// 生成脚本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            sqlBuilder.Append(SQL_DELETE + TableName);
            sqlBuilder.Append(SQL_WHERE);
            sqlBuilder.Append(string.Join(SQL_AND, WhereFields));
            return sqlBuilder.ToString();
        }
    }
}
