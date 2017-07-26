using System.Collections.Generic;
using System.Linq;

namespace Framework.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateSql : SqlBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        public UpdateSql(string tableName)
            : base(tableName, ExecuteSqlType.Update)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static UpdateSql FromTable(string tableName)
        {
            return new UpdateSql(tableName);
        }

        /// <summary>
        /// <para>更新字段</para>
        /// <para>格式写法：[FieldName=ParameterName] OR [FieldName=ParameterValue] OR [FieldName]</para>
        /// <para>Example：SetFieldValues(new string[]{"Nick","Sex=@Sex","Age=22"},new object[]{nick,sex})</para>
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        public UpdateSql SetField(string[] fields, object[] values)
        {
            Fields = ReproduceParameterNames(fields).ToArray();
            Values = values;
            var parameters = new List<string>();
            Fields.ToList().ForEach(n =>
            {
                if (regexParameter.IsMatch(n))
                {
                    parameters.Add(regexParameter.Match(n).Value);
                }
            });
            AddParameters(parameters.ToArray(), values);
            return this;
        }

        /// <summary>
        /// <para>更新字段</para>
        /// <para>Example：SetFieldValues("Nick=@Nick,Sex=@Sex,Age=22",new object[]{nick,sex})</para>
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="values"></param>
        public UpdateSql SetField(string fieldExpression, params object[] values)
        {
            Fields = fieldExpression.Split(',');
            Values = values;
            var parameters = new List<string>();
            Fields.ToList().ForEach(n =>
            {
                if (regexParameter.IsMatch(n))
                {
                    parameters.Add(regexParameter.Match(n).Value);
                }
            });
            AddParameters(parameters.ToArray(), values);
            return this;
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
        public UpdateSql Where(string[] fields,params object[] values)
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
        public UpdateSql Where(string fieldExpression, params object[] values)
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
            sqlBuilder.AppendFormat(SQL_UPDATE, TableName);
            sqlBuilder.Append(string.Join(",", Fields.ToArray()));
            sqlBuilder.Append(SQL_WHERE);
            sqlBuilder.Append(string.Join(SQL_AND, WhereFields));
            return sqlBuilder.ToString();
        }
    }
}
