using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Keede.DAL.RWSplitting;

namespace Keede.SQLHelper
{
    /// <summary>
    /// 数据库脚本操作类
    /// </summary>
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
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdText">数据库执行指令</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        public static int ExecuteNonQuery(string connectionString, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteNonQuery(connectionString, CommandType.Text, cmdText, commandParameters);
        }

        /// <summary>
        /// 指定数据库连接字符串，以执行数据库访问。并返回受影响的数据行数。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">数据库执行指令</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            try
            {
                return Monitor.Run(() =>
                {
                    using (var conn = IsRead(cmdText) ? Databases.GetSqlConnection() : Databases.GetSqlConnection(false))
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                        int val = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return val;
                    }
                }, Databases.GetDbConnectionStr(), cmdText, commandParameters);
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
        public static void ExecuteNonQuery(SqlTransaction trans, string cmdText, params SqlParameter[] commandParameters)
        {
            ExecuteNonQuery(trans, CommandType.Text, cmdText, commandParameters);
        }

        /// <summary>
        /// 传入执行事务，执行数据库访问，并返回受影响的数据行数。
        /// </summary>
        /// <param name="trans">现有的Sql事务</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        public static void ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            try
            {
                SqlCommand cmd = trans.Connection.CreateCommand();
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
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

        #region -- ExecuteDataTable

        /// <summary>
        /// 执行查询返回datatable数据
        /// </summary>
        /// <param name="connectionString">已有的SqlConnection数据库链接</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回 DataTable 类型数据</returns>
        public static DataTable ExecuteDataTable(string connectionString, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteDataTable(connectionString, CommandType.Text, cmdText, commandParameters);
        }

        /// <summary>
        /// 执行查询返回datatable数据
        /// </summary>
        /// <param name="connectionString">已有的SqlConnection数据库链接</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回 DataTable 类型数据</returns>
        public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            try
            {
                return Monitor.Run(() =>
                {
                    using (var conn = Databases.GetSqlConnection() )
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var ds = new DataSet();
                            try
                            {
                                da.Fill(ds, "table");
                                cmd.Parameters.Clear();
                            }
                            catch (SqlException ex)
                            {
                                throw new Exception(ex.Message);
                            }
                            return ds.Tables[0];
                        }
                    }
                }, Databases.GetDbConnectionStr(), cmdText, commandParameters);
            }
            catch (Exception exp)
            {
                string errorMsg = exp.Message + "\r\n";
                var sqlText = GetSQLParamsText(cmdText, commandParameters);
                CreateErrorMsg(errorMsg + sqlText);
                throw exp;
            }
        }

        #endregion -- ExecuteDataTable

        #region -- ExecuteReader

        /// <summary>
        /// 返回数据库查询结果
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteReader(connectionString, CommandType.Text, cmdText, commandParameters);
        }

        /// <summary>
        /// 返回数据库查询结果
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteReader(connectionString, 15, cmdType, cmdText, commandParameters);
        }

        /// <summary>
        /// 返回数据库查询结果,设定超时时间,用于耗时的查询
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        public static SqlDataReader ExecuteReader(string connectionString, int timeOut, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return Monitor.Run(() =>
                {
                    SqlDataReader rdr = null;
                    var conn = Databases.GetSqlConnection();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandTimeout = timeOut;
                    //返回SqlDataReader结果需要开启SqlConnection
                    //使用CommandBehavior返回结果，关闭SqlDataReader对象时同时关闭相关联的SqlConnection对象
                    try
                    {
                        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                        rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        cmd.Parameters.Clear();
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
                }, Databases.GetDbConnectionStr(), cmdText, commandParameters);
        }


        /// <summary>
        /// 返回数据库查询结果,设定超时时间,用于耗时的查询
        /// </summary>
        /// <param name="conn"> 数据库链接对象</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        public static SqlDataReader ExecuteReader(SqlConnection conn, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return Monitor.Run(() =>
            {
                conn = Databases.GetSqlConnection();
                SqlDataReader rdr = null;
                SqlCommand cmd = conn.CreateCommand();
                try
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    rdr = cmd.ExecuteReader();
                    cmd.Parameters.Clear();
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
                    throw exp;
                }
            }, conn.ConnectionString, cmdText, commandParameters);
        }

        #endregion -- ExecuteReader

        #region -- ExecuteScalar

        /// <summary>
        /// 传入数据库连接字符串，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// uparameters 类型参数传递.
        /// </summary>
        /// <remarks>
        /// 函数实例:
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        public static object ExecuteScalar(string connectionString, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(connectionString, CommandType.Text, cmdText, commandParameters);
        }

        /// <summary>
        /// 传入数据库连接字符串，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecuteScalar(connectionString, cmdType, cmdText, 15, commandParameters);
        }

        /// <summary>
        /// 大规模查询，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="timeOut">数据运行超时时间</param>
        /// <param name="commandParameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, int timeOut, params SqlParameter[] commandParameters)
        {
            try
            {
                return Monitor.Run(() =>
                {
                    using (var connection = IsRead(cmdText) ? Databases.GetSqlConnection() : Databases.GetSqlConnection(false))
                    {
                        SqlCommand cmd = connection.CreateCommand();
                        cmd.CommandTimeout = timeOut;
                        PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                        object val = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        return val;
                    }
                }, Databases.GetDbConnectionStr(), cmdText, commandParameters);
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
        /// <param name="cmdText">指令字符串</param>
        /// <param name="cmdParms">存储过程参数</param>
        internal static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, IEnumerable<SqlParameter> cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;
            cmd.Parameters.Clear();
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                {
                    if (parm != null)
                    {
                        if ((parm.Direction == ParameterDirection.InputOutput ||
                            parm.Direction == ParameterDirection.Input) &&
                            (parm.Value == null))
                        {
                            parm.Value = DBNull.Value;
                        }
                        cmd.Parameters.Add(parm);
                    }
                }
            }
        }

        #endregion -- PrepareCommand

        #region -- log

        /// <summary>
        /// 获取带有参数的脚本执行文本内容
        /// </summary>
        /// <param name="cmdText"></param>
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
                LogHelper.Log("SQL.Error", ErrorFilePath, title, message);
            }
            else
            {
                LogHelper.Log("SQL.Error", title, message);
            }
        }

        #endregion -- log

        /// <summary>
        /// 批量插入，手动映射表字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionString"> </param>
        /// <param name="data">数据源</param>
        /// <param name="tableName">插入的表名</param>
        /// <param name="mappings">Key是模型中的字段名，Value是对应数据表中的字段名</param>
        public static Int32 BatchInsert<T>(string connectionString, IEnumerable<T> data, string tableName, Dictionary<string, string> mappings)
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
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, string[] propertyName)
        {
            return ToDataTable(list.ToList(), propertyName);
        }

        private static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}