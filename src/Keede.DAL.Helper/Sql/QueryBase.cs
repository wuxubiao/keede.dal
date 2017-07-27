using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Keede.DAL.Helper.Sql
{
    /// <summary>
    ///
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public abstract class QueryBase
    {
        internal Regex RegexParameter = new Regex(@"@\w+");
        internal readonly StringBuilder SQLBuilder = new StringBuilder();
        internal const string SQL_INSERT_INTO = " INSERT INTO ";
        internal const string SQL_VALUES = " VALUES ";
        internal const string LEFT_BRACKET = " ( ";
        internal const string RIGHT_BRACKET = " ) ";
        internal const string SQL_UPDATE = " UPDATE {0} SET ";
        internal const string SQL_WHERE = " WHERE ";
        internal const string SQL_DELETE = " DELETE ";
        internal const string SQL_AND = " AND ";
        internal const string SQL_OR = " OR ";
        internal const string SQL_SELECT = @" SELECT ";
        internal const string SQL_FROM = " FROM ";

        internal static readonly Regex RegexColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        ///
        /// </summary>
        /// <param name="selectQuery"></param>
        protected QueryBase(string selectQuery)
        {
            SelectQuery = selectQuery;
        }

        #region -- Property

        ///// <summary>
        /////
        ///// </summary>
        //public IList<Parameter> ParameterList { get; private set; }

        internal string PageQueryString
        {
            get
            {
                return @"
WITH TMP AS
(
	{select_query}
)
SELECT * FROM
	(
		SELECT *,ROW_NUMBER() OVER(ORDER BY {field_asc_desc}) AS ROW FROM TMP
	) TB
WHERE ROW BETWEEN {start} AND {end}
";
            }
        }

        /// <summary>
        /// 查询脚本
        /// </summary>
        internal string SelectQuery { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal IList<string> Fields {
            get
            {
                var mt = RegexColumns.Match(SelectQuery);
                if (mt.Success)
                {
                    return mt.Groups[1].Value.Split(',').ToList();
                }
                return new List<string>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        internal IList<string> OrderByFields { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //internal IList<object> Values { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //internal string[] WhereFields { get; set; }

        ///// <summary>
        /////
        ///// </summary>
        //internal object[] WhereValues { get; set; }

        #endregion -- Property

        //internal IEnumerable<string> ConvertParameterNames(string[] fields)
        //{
        //    foreach (var f in fields)
        //    {
        //        yield return "@" + f;
        //    }
        //}

        ///// <summary>
        ///// 复制繁殖参数名称
        ///// </summary>
        ///// <param name="fields"></param>
        ///// <returns></returns>
        //internal IEnumerable<string> ReproduceParameterNames(string[] fields)
        //{
        //    foreach (var f in fields)
        //    {
        //        var fd = f;
        //        if (f.IndexOf('=') < 0)
        //        {
        //            yield return (f + "=@" + fd);
        //        }
        //        else
        //        {
        //            yield return f;
        //        }
        //    }
        //}

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="parameterName"></param>
        ///// <param name="value"></param>
        //internal void AddParameter(string parameterName, object value)
        //{
        //    if (parameterName != string.Empty && ParameterList.All(p => p.Name != parameterName))
        //    {
        //        ParameterList.Add(new Parameter(parameterName, value));
        //    }
        //}

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <param name="values"></param>
        //internal void AddParameters(string[] parameters, object[] values)
        //{
        //    for (var i = 0; i < parameters.Length; i++)
        //    {
        //        if (parameters[i] != string.Empty && ParameterList.All(p => p.Name != parameters[i]))
        //        {
        //            ParameterList.Add(new Parameter(parameters[i], values[i]));
        //        }
        //    }
        //}
    }
}