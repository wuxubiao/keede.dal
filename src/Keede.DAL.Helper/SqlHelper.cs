using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Keede.DAL.RWSplitting;
using Dapper;

namespace Keede.DAL.Helper
{
    /// <summary>
    /// 数据库脚本操作类
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public static partial class SqlHelper
    {
        /// <summary>
        /// 数据库执行错误写入日志文件路径
        /// </summary>
        public static string ErrorFilePath
        {
            get;
            set;
        }

        #region -- ExecuteNonQuery

        /// <summary>
        /// 指定数据库连接字符串，以执行数据库访问。并返回受影响的数据行数。
        /// </summary>
        /// <param>数据库执行模式
        ///     <name>cmdType</name>
        /// </param>
        /// <param name="cmdText">数据库执行指令</param>
        /// <param name="dbName"></param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static int ExecuteNonQuery(string cmdText, params SqlParameter[] commandParameters)
        {
            try
            {
                using (var conn = SqlStatement.IsRead(cmdText) ? Databases.GetSqlConnection() : Databases.GetSqlConnection(false))
                {
                    return conn.Execute(cmdText, commandParameters);
                }
            }
            catch (SqlException exp)
            {
                string errorMsg = exp.Message + "\r\n";
                var sqlText = GetSQLParamsText(cmdText, commandParameters);
                CreateErrorMsg(errorMsg + sqlText);
                throw exp;
            }
        }

        /// <summary>
        /// 传入执行事务，执行数据库访问，并返回受影响的数据行数。
        /// </summary>
        /// <param name="trans">现有的Sql事务</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static void ExecuteNonQuery(SqlTransaction trans, string cmdText, params SqlParameter[] commandParameters)
        {
            try
            {
                trans.Connection.Execute(cmdText, commandParameters);
            }
            catch (SqlException exp)
            {
                string errorMsg = exp.Message + "\r\n";
                var sqlText = GetSQLParamsText(cmdText, commandParameters);
                CreateErrorMsg(errorMsg + sqlText);
                throw exp;
            }
        }

        #endregion -- ExecuteNonQuery

        #region -- ExecuteReader

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="dbName"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static SqlDataReader ExecuteReader(string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteReader(15, cmdText, commandParameters);
        }

        /// <summary>
        /// 返回数据库查询结果,设定超时时间,用于耗时的查询
        /// </summary>
        /// <param name="timeOut">超时时间</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="dbName"></param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static SqlDataReader ExecuteReader(int timeOut, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlDataReader rdr = null;
            var conn = Databases.GetSqlConnection();

            //返回SqlDataReader结果需要开启SqlConnection
            //使用CommandBehavior返回结果，关闭SqlDataReader对象时同时关闭相关联的SqlConnection对象
            try
            {
                rdr=(SqlDataReader) conn.ExecuteReader(cmdText, commandParameters, null, timeOut);
                return rdr;
            }
            catch (SqlException exp)
            {
                string errorMsg = exp.Message + "\r\n";
                var sqlText = GetSQLParamsText(cmdText, commandParameters);
                CreateErrorMsg(errorMsg + sqlText);
                if (rdr != null && rdr.IsClosed)
                {
                    rdr.Close();
                }
                conn.Close();
                throw exp;
            }
            catch (InvalidOperationException exp)
            {
                string errorMsg = exp.Message + "\r\n";
                var sqlText = GetSQLParamsText(cmdText, commandParameters);
                CreateErrorMsg(errorMsg + sqlText);
                if (rdr != null && rdr.IsClosed)
                {
                    rdr.Close();
                }
                conn.Close();
                throw exp;
            }
        }
        #endregion -- ExecuteReader

        #region -- ExecuteScalar

        /// <summary>
        /// 传入数据库连接字符串，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// </summary>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="dbName"></param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static object ExecuteScalar(string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(cmdText, 15, commandParameters);
        }

        /// <summary>
        /// 大规模查询，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// </summary>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="timeOut">数据运行超时时间</param>
        /// <param name="dbName"></param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        public static object ExecuteScalar(string cmdText, int timeOut, params SqlParameter[] commandParameters)
        {
            try
            {
                using (var connection = SqlStatement.IsRead(cmdText) ? Databases.GetSqlConnection() : Databases.GetSqlConnection(false))
                {
                    object val = connection.ExecuteScalar(cmdText, commandParameters, null, timeOut);
                    return val;
                }
            }
            catch (SqlException exp)
            {
                string errorMsg = exp.Message + "\r\n";
                var sqlText = GetSQLParamsText(cmdText, commandParameters);
                CreateErrorMsg(errorMsg + sqlText);
                throw exp;
            }
        }

        #endregion -- ExecuteScalar

        #region -- PrepareCommand

        /// <summary>
        /// 数据库指令执行模块
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">执行模式说明</param>
        /// <param name="cmdParms">存储过程参数</param>
        /// <summary>
        /// 获取带有参数的脚本执行文本内容
        /// </summary>
        /// <param name="cmdText">
        ///     指令字符串
        /// </param>
        /// <param name="cmdParams"></param>
        /// <returns></returns>
        private static string GetSQLParamsText(string cmdText, params SqlParameter[] cmdParams)
        {
            var parmText = new StringBuilder();
            parmText.Append(@"--脚本运行语句\r\n");
            parmText.Append("DECLARE ");
            if (cmdParams != null)
            {
                for (int i = 0; i < cmdParams.Length; i++)
                {
                    var p = cmdParams[i];
                    parmText.Append(p.ParameterName + " ");
                    if (p.SqlDbType == SqlDbType.VarChar || p.SqlDbType == SqlDbType.NVarChar ||
                        p.SqlDbType == SqlDbType.NChar || p.SqlDbType == SqlDbType.Char)
                    {
                        parmText.Append(p.SqlDbType + "(" + p.Size + ")");
                    }
                    else
                    {
                        parmText.Append(p.SqlDbType);
                    }
                    if (i < (cmdParams.Length - 1))
                    {
                        parmText.Append(",");
                    }
                }
                parmText.Append(@"\r\n");
                foreach (var p in cmdParams)
                {
                    parmText.Append("SET " + p.ParameterName + "='" + p.Value + @"' \r\n");
                }
            }
            parmText.Append(cmdText + @"\r\n");
            parmText.Append("--END");
            return parmText.ToString();
        }

        /// <summary>
        /// 创建错误日志
        /// </summary>
        /// <param name="message"></param>
        private static void CreateErrorMsg(string message)
        {
            string title = "[" + DateTime.Now + "] SQL脚本运行错误信息";
            if (!string.IsNullOrEmpty(ErrorFilePath))
            {
                //LogHelper.Log("SQL.Error", ErrorFilePath, title, message);
            }
            else
            {
                //LogHelper.Log("SQL.Error", title, message);
            }
        }

        #endregion -- log

        /// <summary>
        /// 批量插入，手动映射表字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据源</param>
        /// <param name="tableName">插入的表名</param>
        /// <param name="mappings">Key是模型中的字段名，Value是对应数据表中的字段名</param>
        /// <param name="dbName"></param>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static Int32 BatchInsert<T>(IEnumerable<T> data, string tableName, Dictionary<string, string> mappings)
        {
            if (data == null || !data.Any())
                return 0;

            using (var conn = Databases.GetSqlConnection(false))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var sqlBulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction)
                        {
                            BatchSize = data.Count(),
                            DestinationTableName = tableName
                        };

                        foreach (var mapping in mappings)
                        {
                            sqlBulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
                        }

                        sqlBulkCopy.BulkCopyTimeout = int.MaxValue;
                        sqlBulkCopy.WriteToServer(data.ToDataTable());
                        transaction.Commit();
                        sqlBulkCopy.Close();
                        return sqlBulkCopy.BatchSize;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction"></param>
        /// <param name="items"></param>
        /// <param name="tableName"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static Int32 BatchInsert<T>(SqlTransaction transaction, IEnumerable<T> items, string tableName, Dictionary<string, string> mappings)
        {
            var data = items.ToList();
            if (data == null || !data.Any())
                return 0;

            try
            {
                var sqlBulkCopy = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction)
                {
                    BatchSize = data.Count(),
                    DestinationTableName = tableName
                };

                foreach (var mapping in mappings)
                {
                    sqlBulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
                }

                sqlBulkCopy.BulkCopyTimeout = int.MaxValue;

                sqlBulkCopy.WriteToServer(data.ToDataTable());

                sqlBulkCopy.Close();
                return sqlBulkCopy.BatchSize;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            var result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, IsNullableType(pi.PropertyType) ? Nullable.GetUnderlyingType(pi.PropertyType) : pi.PropertyType);
                }

                foreach (object item in list)
                {
                    object temp = item;
                    var values = propertys.Select(propertyInfo => propertyInfo.GetValue(temp, null)).ToArray();
                    result.LoadDataRow(values, true);
                }
            }
            return result;
        }

        /// <summary>
        /// 将泛型List类的指定列转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <param name="propertyName">需要返回的列的列名</param>
        /// <returns>数据集(表)</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static DataTable ToDataTable<T>(this IList<T> list, string[] propertyName)
        {
            var propertyNameList = propertyName ?? new string[0];

            var result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (var pi in propertys.Where(pi => propertyNameList.Contains(pi.Name)))
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }

                foreach (T item in list)
                {
                    result.LoadDataRow(propertys.Where(pi => propertyNameList.Contains(pi.Name)).Select(property => property.GetValue(item, null)).ToArray(), true);
                }
            }
            return result;
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <param name="list">集合</param>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            return ToDataTable(list.ToList());
        }

        /// <summary>
        /// 将泛型集合类的指定列转换成DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, string[] propertyName)
        {
            return ToDataTable(list.ToList(), propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}