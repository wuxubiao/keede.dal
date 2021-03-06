﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Keede.DAL.Helper.Mapper;
using Keede.DAL.Helper.Sql;

namespace Keede.DAL.Helper
{
    /// <summary>
    ///
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class Database : DbHelper
    {
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
        public Database()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbName"></param>
        public Database(string dbName)
            : base(dbName)
        {
        }

        ///  <summary>
        /// 
        ///  </summary>
        /// <param name="exception"></param>
        /// <param name="dbName"></param>
        public Database(DbExecuteException exception, string dbName = null)
            : base(exception,dbName)
        {
        }
        #endregion -- Init()

        #region -- 连项调用执行脚本
        /// <summary>
        ///
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
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
        [Obsolete("This function is obsolete,don't use it in new project")]
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
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public bool Execute(bool isReadDb=true)
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return Execute(isReadDb, sql, parameters);
        }

        /// <summary>
        /// 执行数据操作
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public bool Execute(bool isReadDb, string sql, params Parameter[] parameters)
        {
            return ExecuteNonQuery(isReadDb, sql, parameters) > 0;
        }

        #endregion -- Execute()

        #region -- GetValue()

        /// <summary>
        /// 获取一个值类型的数据
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public TValue GetValue<TValue>(bool isReadDb=true)
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return GetValue<TValue>(isReadDb, sql, parameters);
        }

        ///  <summary>
        /// 
        ///  </summary>
        /// <param name="isReadDb"></param>
        /// <param name="sql"></param>
        ///  <param name="parameters"></param>
        ///  <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public TValue GetValue<TValue>(bool isReadDb, string sql, params Parameter[] parameters)
        {
            var value = ExecuteScalar(isReadDb, sql, parameters);
            if (value != null)
            {
                return (TValue)value;
            }
            return default(TValue);
        }

        #endregion -- GetValue()

        #region -- GetValues()

        /// <summary>
        /// 获取一个值类型的集合数据
        /// <para>
        /// 注释：调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public IEnumerable<TValue> GetValues<TValue>(bool isReadDb = true)
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return GetValues<TValue>(isReadDb, sql, parameters);
        }

        /// <summary>
        /// 获取一个值类型的集合数据
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public IList<TValue> GetValues<TValue>(bool isReadDb, string sql, params Parameter[] parameters)
        {
            var items = new List<TValue>();
            using (var reader = ExecuteReader(isReadDb, sql, parameters))
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
        }

        #endregion -- GetValues()

        #region -- Single()

        /// <summary>
        /// 获取单个数据模型
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public T Single<T>(bool isReadDb = true) where T : class, new()
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return Single<T>(isReadDb, sql, parameters);
        }

        ///  <summary>
        /// 
        ///  </summary>
        ///  <param name="isReadDb"></param>
        ///  <param name="sql"></param>
        ///  <param name="parameters"></param>
        ///  <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public T Single<T>(bool isReadDb, string sql, params Parameter[] parameters) where T : class, new()
        {
            var type = typeof(T);
            var dataReader = DataReader.Create(type);
            if (dataReader != null)
            {
                using (var reader = ExecuteReader(isReadDb, sql, parameters))
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
        }

        #endregion -- Single()

        #region -- Select()

        /// <summary>
        /// 获取数据集合
        /// <para>
        /// 调用此方法，请先调用Run和AddParameter加入运行的脚本和参数
        /// </para>
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public IEnumerable<T> Select<T>(bool isReadDb = true) where T : class, new()
        {
            var sql = CurrentSQL;
            var parameters = CurrentParameter.ToArray();
            return Select<T>(isReadDb, sql, parameters);
        }

        ///  <summary>
        /// 
        ///  </summary>
        /// <param name="isReadDb"></param>
        /// <param name="sql"></param>
        ///  <param name="parameters"></param>
        ///  <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public IEnumerable<T> Select<T>(bool isReadDb, string sql, params Parameter[] parameters) where T : class, new()
        {
            var type = typeof(T);
            var items = new List<T>();
            var dataReader = DataReader.Create(type);
            if (dataReader != null)
            {
                using (var reader = ExecuteReader(isReadDb, sql, parameters))
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
        }

        #endregion -- Select()

        #region -- SelectByPage()

        /// <summary>
        /// 获取翻页实体对象信息，内带有数据集合
        /// </summary>
        /// <typeparam name="T">泛类型（请传递引用类型）</typeparam>
        /// <param name="isReadDb"></param>
        /// <param name="query">数据脚本</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public PageItems<T> SelectByPage<T>(bool isReadDb, PageQuery query, params Parameter[] parameters) where T : class, new()
        {
            var recordcount = GetValue<int>(isReadDb, query.ToCountQuery(), parameters);
            var items = Select<T>(isReadDb, query.ToFullQuery(), parameters);
            return new PageItems<T>((int)(query.EndRow - query.StartRow) + 1, recordcount, items);
        }

        /// <summary>
        /// 获取翻页实体对象信息，内带有数据集合
        /// </summary>
        /// <typeparam name="T">泛类型（请传递引用类型）</typeparam>
        /// <param name="isReadDb"></param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页数</param>
        /// <param name="sql">数据脚本</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public PageItems<T> SelectByPage<T>(bool isReadDb, int pageIndex, int pageSize, string sql, params Parameter[] parameters)
            where T : class, new()
        {
            long start = 0;
            if (pageIndex <= 1)
            {
                pageIndex = 0;
            }
            else
            {
                start = (pageIndex - 1) * pageSize;
            }
            string sqlPage;
            string sqlCount;
            BuildPageQueries(start, pageSize, sql, out sqlCount, out sqlPage);
            var recordcount = GetValue<int>(isReadDb, sqlCount, parameters);
            var items = Select<T>(isReadDb, sqlPage, parameters);
            return new PageItems<T>(pageIndex, pageSize, recordcount, items);
        }

        /// <summary>
        /// 获取翻页实体对象信息，内带有数据集合
        /// </summary>
        /// <typeparam name="T">泛类型（请传递引用类型）</typeparam>
        /// <param name="isReadDb"></param>
        /// <param name="start">起始数据记录索引</param>
        /// <param name="limit">限制读取条数</param>
        /// <param name="sql">数据脚本</param>
        /// <param name="parameters">参数集合</param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public PageItems<T> SelectByPage<T>(bool isReadDb, long start, long limit, string sql, params Parameter[] parameters)
            where T : class, new()
        {
            string sqlPage;
            string sqlCount;
            BuildPageQueries(start, limit, sql, out sqlCount, out sqlPage);
            if (sqlPage.ToLower().LastIndexOf("order by", StringComparison.Ordinal) > 0)
            {
                sqlPage += " ORDER BY 1";
            }
            var recordcount = GetValue<int>(isReadDb, sqlCount, parameters);
            var items = Select<T>(isReadDb, sqlPage, parameters);
            return new PageItems<T>((int)limit, recordcount, items);
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