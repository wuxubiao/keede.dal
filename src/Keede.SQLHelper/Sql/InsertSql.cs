using System.Linq;

namespace Framework.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class InsertSql:SqlBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        public InsertSql(string tableName) : base(tableName,ExecuteSqlType.Insert)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static InsertSql FromTable(string tableName)
        {
            return new InsertSql(tableName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        public InsertSql SetFieldsAndValues(string[] fields, object[] values)
        {
            if (fields.Length != values.Length)
            {
                throw new System.ApplicationException(string.Format(ErrorMessage.METHOD_EXCEPTION, "InsertSql",
                                                                    "SetFieldsAndValues", "参数:fields和values数量不一致！"));
            }
            Fields = fields;
            Values = values;
            AddParameters(fields, values);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public InsertSql SetField(string field, object value)
        {
            Fields.Add(field);
            Values.Add(value);
            AddParameter(field, value);
            return this;
        }

        /// <summary>
        /// 生成脚本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            sqlBuilder.Append(SQL_INSERT_INTO);
            sqlBuilder.Append(LEFT_BRACKET + string.Join(",", Fields.ToArray()) + RIGHT_BRACKET);
            sqlBuilder.Append(SQL_VALUES);
            sqlBuilder.Append(LEFT_BRACKET + string.Join(",", ConvertParameterNames(Fields.ToArray()).ToArray()) + RIGHT_BRACKET);
            return sqlBuilder.ToString();
        }
    }
}
