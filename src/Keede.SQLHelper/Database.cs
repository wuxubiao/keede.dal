using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Keede.SQLHelper.Mapper;
using Keede.SQLHelper.Sql;

namespace Keede.SQLHelper
{
    /// <summary>
    ///
    /// </summary>
    public class Database : DbHelper
    {
        #region -- 静态构造函数
        static Database()
        {
            var needMonitorSQL = System.Configuration.ConfigurationManager.AppSettings["NeedMonitorSQL"];
            if (needMonitorSQL.ToLower() == "true")
            {
                IsNeedMonitor = true;
            }
            else
            {
                IsNeedMonitor = false;
            }
        }
        #endregion

        #region -- 静态定义

        /// <summary>
        ///
        /// </summary>
        public static bool IsNeedMonitor { get; internal set; }

        #endregion -- 静态定义

        #region -- 定义属性
        /// <summary>
        /// 当前执行的脚本
        /// </summary>
        protected string CurrentSQL
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前要传入的参数
        /// </summary>
        protected IList<Parameter> CurrentParameter
        {
            get;
            private set;
        }
        #endregion

        #region -- Init()

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionName"></param>
        public Database(string connectionName)
            : base(connectionName)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="exception"></param>
        public Database(string connectionName, DbExecuteException exception)
            : base(connectionName, exception)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="connectionString"></param>
        public Database(string providerName, string connectionString)
            : base(providerName, connectionString)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="connectionString"></param>
        /// <param name="exception"></param>
        public Database(string providerName, string connectionString, DbExecuteException exception)
            : base(providerName, connectionString, exception)
        {
        }

        #endregion -- Init()

        #region -- 连项调用执行脚本
        /// <summary>
        ///
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public Database Run(string sql)
        {
            if (CurrentParameter != null)
            {
                CurrentParameter.Clear();
            }
            else
            {
                CurrentParameter = new List<Parameter>();
            }
            CurrentSQL = sql;
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public Database AddParameter(string parameterName, object parameterValue)
        {
            CurrentParameter.Add(new Parameter(parameterName, parameterValue));
            return this;
        }
        #endregion

        #region -- Execute()

        /// <summary>
        /// 执行数据操作  
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return Execute(CommandType.Text, sql, parameters);
        }

        /// <summary>
        /// 执行数据操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool Execute(string sql, params Parameter[] parameters)
        {
            return Execute(CommandType.Text, sql, parameters);
        }

        /// <summary>
        /// 执行数据操作
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool Execute(CommandType commandType, string sql, params Parameter[] parameters)
        {
            return ExecuteNonQuery(commandType, sql, parameters) > 0;
        }

        #endregion -- Execute()

        #region -- GetValue()

        /// <summary>
        /// 获取一个值类型的数据
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        public TValue GetValue<TValue>()
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return GetValue<TValue>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TValue GetValue<TValue>(string sql, params Parameter[] parameters)
        {
            return GetValue<TValue>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TValue GetValue<TValue>(CommandType commandType, string sql, params Parameter[] parameters) 
        {
            return Monitor.Run(() =>
                {
                    var value = ExecuteScalar(commandType, sql, parameters);
                    if (value != null)
                    {
                        return (TValue)value;
                    }
                    return default(TValue);
                }, ConnectionString, sql, parameters);
        }

        #endregion -- GetValue()

        #region -- GetValues()

        /// <summary>
        /// 获取一个值类型的集合数据
        /// <para>
        /// 注释：调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues<TValue>() 
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return GetValues<TValue>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        /// 获取一个值类型的集合数据    
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues<TValue>(string sql, params Parameter[] parameters) 
        {
            return GetValues<TValue>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        /// 获取一个值类型的集合数据
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IList<TValue> GetValues<TValue>(CommandType commandType, string sql, params Parameter[] parameters) 
        {
            return Monitor.Run(() =>
                {
                    var items = new List<TValue>();
                    using (var reader = ExecuteReader(commandType, sql, parameters))
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                var value = reader[0];
                                if (value != DBNull.Value)
                                {
                                    items.Add((TValue)value);
                                }
                            }
                        }
                    }
                    return items;
                }, ConnectionString, sql, parameters);
        }

        #endregion -- GetValues()

        #region -- Single()

        /// <summary>
        /// 获取单个数据模型
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        public T Single<T>() where T : class,new()
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return Single<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Single<T>(string sql, params Parameter[] parameters) where T : class,new()
        {
            return Single<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Single<T>(CommandType commandType, string sql, params Parameter[] parameters) where T : class,new()
        {
            return Monitor.Run(() =>
                {
                    var type = typeof(T);
                    var dataReader = DataReader.Create(type);
                    if (dataReader != null)
                    {
                        using (var reader = ExecuteReader(commandType, sql, parameters))
                        {
                            if (reader.Read())
                            {
                                object item = Activator.CreateInstance(type);
                                dataReader.ReaderToObject(reader, ref item);
                                return (T)item;
                            }
                        }
                    }
                    return default(T);
                }, ConnectionString, sql, parameters);
        }

        #endregion -- Single()

        #region -- Select()

        /// <summary>
        /// 获取数据集合
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Select<T>() where T : class,new()
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return Select<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(string sql, params Parameter[] parameters) where T : class,new()
        {
            return Select<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(CommandType commandType, string sql, params Parameter[] parameters) where T : class,new()
        {
            return Monitor.Run(() =>
                {
                    var type = typeof(T);
                    var items = new List<T>();
                    var dataReader = DataReader.Create(type);
                    if (dataReader != null)
                    {
                        using (var reader = ExecuteReader(commandType, sql, parameters))
                        {
                            while (reader.Read())
                            {
                                object item = Activator.CreateInstance(type);
                                dataReader.ReaderToObject(reader, ref item);
                                items.Add((T)item);
                            }
                        }
                    }
                    return items;
                }, ConnectionString, sql, parameters);
        }

        #endregion -- Select()

        #region -- SelectByPage()

        /// <summary>
        /// 获取翻页实体对象信息，内带有数据集合
        /// </summary>
        /// <typeparam name="T">泛类型（请传递引用类型）</typeparam>
        /// <param name="query">数据脚本</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        public PageItems<T> SelectByPage<T>(PageQuery query, params Parameter[] parameters) where T : class, new()
        {
            var recordcount = GetValue<int>(query.ToCountQuery(), parameters);
            var items = Select<T>(query.ToFullQuery(), parameters);
            return new PageItems<T>((int) (query.EndRow - query.StartRow) + 1, recordcount, items);
        }

        /// <summary>
        /// 获取翻页实体对象信息，内带有数据集合
        /// </summary>
        /// <typeparam name="T">泛类型（请传递引用类型）</typeparam>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页数</param>
        /// <param name="sql">数据脚本</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        public PageItems<T> SelectByPage<T>(int pageIndex, int pageSize, string sql, params Parameter[] parameters)
            where T : class, new()
        {
            long start = 0;
            if (pageIndex <= 1)
            {
                pageIndex = 0;
            }
            else
            {
                start = (pageIndex - 1)*pageSize;
            }
            string sqlPage;
            string sqlCount;
            BuildPageQueries(start, pageSize, sql, out sqlCount, out sqlPage);
            var recordcount = GetValue<int>(sqlCount, parameters);
            var items = Select<T>(sqlPage, parameters);
            return new PageItems<T>(pageIndex, pageSize, recordcount, items);
        }

        /// <summary>
        /// 获取翻页实体对象信息，内带有数据集合
        /// </summary>
        /// <typeparam name="T">泛类型（请传递引用类型）</typeparam>
        /// <param name="start">起始数据记录索引</param>
        /// <param name="limit">限制读取条数</param>
        /// <param name="sql">数据脚本</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        public PageItems<T> SelectByPage<T>(long start, long limit, string sql, params Parameter[] parameters)
            where T : class, new()
        {
            string sqlPage;
            string sqlCount;
            BuildPageQueries(start, limit, sql, out sqlCount, out sqlPage);
            if (sqlPage.ToLower().LastIndexOf("order by", StringComparison.Ordinal) > 0)
            {
                sqlPage += " ORDER BY 1";
            }
            var recordcount = GetValue<int>(sqlCount, parameters);
            var items = Select<T>(sqlPage, parameters);
            return new PageItems<T>((int) limit, recordcount, items);
        }

        #endregion -- SelectByPage()

        #region -- PageBuilder

        internal static readonly Regex RegexColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        internal static readonly Regex RegexOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        internal static readonly Regex RegexDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        ///
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlCount"></param>
        /// <param name="sqlSelectRemoved"></param>
        /// <param name="sqlOrderBy"></param>
        /// <returns></returns>
        internal bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = string.Empty;
            sqlCount = string.Empty;
            sqlOrderBy = string.Empty;

            // Extract the columns from "SELECT <whatever> FROM"
            var coloumn = RegexColumns.Match(sql);
            if (!coloumn.Success)
                return false;

            // Save column list and replace with COUNT(*)
            Group coloumngroup = coloumn.Groups[1];
            sqlSelectRemoved = sql.Substring(coloumngroup.Index);

            if (RegexDistinct.IsMatch(sqlSelectRemoved))
                sqlCount = sql.Substring(0, coloumngroup.Index) + "COUNT(" + coloumn.Groups[1].ToString().Trim() + ") " + sql.Substring(coloumngroup.Index + coloumngroup.Length);
            else
                sqlCount = sql.Substring(0, coloumngroup.Index) + "COUNT(*) " + sql.Substring(coloumngroup.Index + coloumngroup.Length);

            // Look for an "ORDER BY <whatever>" clause
            var orderby = RegexOrderBy.Match(sqlCount);
            if (orderby.Success)
            {
                var orderbygroup = orderby.Groups[0];
                sqlOrderBy = orderbygroup.ToString();
                sqlCount = sqlCount.Substring(0, orderbygroup.Index) +
                           sqlCount.Substring(orderbygroup.Index + orderbygroup.Length);
            }
            else
            {
                var n = sqlCount.ToUpper().LastIndexOf("ORDER BY", StringComparison.Ordinal);
                if (n > -1)
                {
                    sqlOrderBy = sqlCount.Substring(n);
                    sqlCount = sqlCount.Substring(0, n);
                    sqlSelectRemoved = sqlSelectRemoved.Replace(sqlOrderBy, string.Empty);
                }
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sql"></param>
        /// <param name="sqlCount"></param>
        /// <param name="sqlPage"></param>
        internal void BuildPageQueries(long skip, long take, string sql, out string sqlCount,
                                        out string sqlPage)
        {
            sqlPage = string.Empty;

            // Split the SQL into the bits we need
            string sqlSelectRemoved, sqlOrderBy;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
                throw new Exception("Unable to parse SQL statement for paged query");

            // Build the SQL for the actual final result
            //if (Connection.GetType().Namespace == "System.Data.SqlClient")
            //{
            sqlSelectRemoved = RegexOrderBy.Replace(sqlSelectRemoved, "");
            if (RegexDistinct.IsMatch(sqlSelectRemoved))
            {
                sqlSelectRemoved = "peta_inner.* FROM (SELECT " + sqlSelectRemoved + ") peta_inner";
            }
            sqlPage = string.Format(
                "SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) AS ROW, {1}) PageTable WHERE ROW>{2} AND ROW<={3}",
                sqlOrderBy ?? "ORDER BY (SELECT NULL)",
                sqlSelectRemoved,
                skip, skip + take);
            //}
        }

        #endregion -- PageBuilder
    }
}