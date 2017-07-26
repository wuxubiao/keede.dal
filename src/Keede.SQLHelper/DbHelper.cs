using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Keede.SQLHelper
{
    /// <summary>
    /// SqlHelper 用于封装数据库链接,以及基本操作的访问。
    /// 此控件为Sql Server操作封装。
    /// </summary>
    public class DbHelper : IDisposable
    {
        internal IDbConnection CurrentConnection { get; set; }

        internal IDbTransaction Transaction { get; set; }

        internal IDbCommand Command { get; set; }

        internal bool IsOpenTransaction { get; set; }

        internal string ConnectionName { get;private set; }

        internal string ProviderName { get; set; }

        internal string ConnectionString { get; set; }

        internal StringBuilder CommandTextBuilder { get; set; }

        /// <summary>
        ///
        /// </summary>
        internal DbExecuteException OnDbExecuteException
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionName"></param>
        internal DbHelper(string connectionName)
        {
            ConnectionName = connectionName;
            var config = Configuration.ConnectionStrings[connectionName];
            if (config.ConnectionString == string.Empty)
            {
                throw new ApplicationException("数据库配置命：" + connectionName + " 不存在！");
            }
            Init(config.ProviderName, config.ConnectionString, null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="exception"></param>
        internal DbHelper(string connectionName, DbExecuteException exception)
        {
            ConnectionName = connectionName;
            var config = Configuration.ConnectionStrings[connectionName];
            if (config.ConnectionString == string.Empty)
            {
                throw new ApplicationException("数据库配置命：" + connectionName + " 不存在！");
            }
            Init(config.ProviderName, config.ConnectionString, exception);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="connectionString"></param>
        internal DbHelper(string providerName, string connectionString)
        {
            ConnectionName = string.Empty;
            Init(providerName, connectionString, null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="connectionString"></param>
        /// <param name="exception"></param>
        internal DbHelper(string providerName, string connectionString, DbExecuteException exception)
        {
            ConnectionName = string.Empty;
            Init(providerName, connectionString, exception);
        }

        private void Init(string providerName, string connectionString, DbExecuteException exception)
        {
            ProviderName = providerName;
            ConnectionString = connectionString;
            OnDbExecuteException = exception;
        }

        /// <summary>
        /// 指定数据库连接字符串，以执行数据库访问。并返回受影响的数据行数。
        /// parameters 类型参数传递。
        /// </summary>
        /// <param name="cmdText">数据库执行指令</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        internal int ExecuteNonQuery(string cmdText, params Parameter[] parameters)
        {
            return ExecuteNonQuery(CommandType.Text, cmdText, parameters);
        }

        /// <summary>
        /// 传入执行事务，执行数据库访问，并返回受影响的数据行数。
        /// uparameters 类型参数传递。
        /// </summary>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        internal int ExecuteNonQuery(CommandType cmdType, string cmdText, params Parameter[] parameters)
        {
            IDbConnection conn = IsOpenTransaction ? CurrentConnection : DbFactory.CreateConnection(ProviderName, ConnectionString);
            try
            {
                var cmd = CreateCommand(conn, cmdType, cmdText, parameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
            catch (SqlException exp)
            {
                if (OnDbExecuteException != null)
                {
                    OnDbExecuteException(new DbExceptionInfo(exp, cmdText, parameters));
                }
                throw exp;
            }
            catch (InvalidOperationException exp)
            {
                if (OnDbExecuteException != null)
                {
                    OnDbExecuteException(new DbExceptionInfo(exp, cmdText, parameters));
                }
                throw exp;
            }
            finally
            {
                if (!IsOpenTransaction)
                {
                    CloseConnection(conn);
                }
            }
        }

        /// <summary>
        /// 返回数据库查询结果
        /// uparameters 类型参数传递.
        /// </summary>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        internal IDataReader ExecuteReader(string cmdText, params Parameter[] parameters)
        {
            return ExecuteReader(CommandType.Text, cmdText, parameters);
        }

        /// <summary>
        /// 返回数据库查询结果,设定超时时间,用于耗时的查询
        /// </summary>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        internal IDataReader ExecuteReader(CommandType cmdType, string cmdText, params Parameter[] parameters)
        {
            try
            {
                IDbConnection conn = IsOpenTransaction ? CurrentConnection : DbFactory.CreateConnection(ProviderName, ConnectionString);
                var cmd = CreateCommand(conn, cmdType, cmdText, parameters);
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return reader;
            }
            catch (SqlException exp)
            {
                if (OnDbExecuteException != null)
                {
                    OnDbExecuteException(new DbExceptionInfo(exp, cmdText, parameters));
                }
                throw exp;
            }
        }

        /// <summary>
        /// 大规模查询，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// uparameters 类型参数传递.
        /// </summary>
        /// <remarks>
        /// 函数实例:
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        internal object ExecuteScalar(string cmdText, params Parameter[] parameters)
        {
            return ExecuteScalar(CommandType.Text, cmdText, parameters);
        }

        /// <summary>
        /// 传入现有数据库连接，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// using the provided parameters.
        /// </summary>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        internal object ExecuteScalar(CommandType cmdType, string cmdText, params Parameter[] parameters)
        {
            IDbConnection conn = IsOpenTransaction ? CurrentConnection : DbFactory.CreateConnection(ProviderName, ConnectionString);
            try
            {
                var cmd = CreateCommand(conn, cmdType, cmdText, parameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
            catch (SqlException exp)
            {
                if (OnDbExecuteException != null)
                {
                    OnDbExecuteException(new DbExceptionInfo(exp, cmdText, parameters));
                }
                throw exp;
            }
            finally
            {
                if (!IsOpenTransaction)
                {
                    CloseConnection(conn);
                }
            }
        }

        /// <summary>
        /// 数据库指令执行模块
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cmdType">执行模式说明</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="cmdParms">存储过程参数</param>
        internal IDbCommand CreateCommand(IDbConnection connection, CommandType cmdType, string cmdText, IEnumerable<Parameter> cmdParms)
        {
            Command = connection.CreateCommand();
            Command.CommandType = cmdType;
            Command.CommandText = cmdText;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (Command.Connection == null)
            {
                Command.Connection = connection;
            }
            if (IsOpenTransaction && Transaction != null)
            {
                Command.Transaction = Transaction;
                if (CommandTextBuilder == null)
                {
                    CommandTextBuilder = new StringBuilder();
                }
                CommandTextBuilder.Append(cmdText + " || ");
            }
            if (cmdParms != null)
            {
                foreach (var p in cmdParms)
                {
                    if (p != null)
                    {
                        var parm = Command.CreateParameter();
                        parm.ParameterName = p.Name;
                        parm.Value = p.Value ?? DBNull.Value;
                        parm.Direction = p.Direction;
                        Command.Parameters.Add(parm);
                    }
                }
            }
            return Command;
        }

        #region -- Transaction()

        /// <summary>
        ///
        /// </summary>
        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary>
        ///
        /// </summary>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (CurrentConnection == null)
            {
                CurrentConnection = DbFactory.CreateConnection(ProviderName, ConnectionString);
            }
            if (CurrentConnection != null)
            {
                if (string.IsNullOrEmpty(CurrentConnection.ConnectionString))
                {
                    CurrentConnection.ConnectionString = ConnectionString;
                }
                if (CurrentConnection.State != ConnectionState.Open)
                {
                    CurrentConnection.Open();
                }
                Transaction = CurrentConnection.BeginTransaction();
            }
            IsOpenTransaction = true;
        }

        /// <summary>
        ///
        /// </summary>
        public bool CompleteTransaction()
        {
            try
            {
                Transaction.Commit();
                IsOpenTransaction = false;
                return true;
            }
            catch (InvalidOperationException exp)
            {
                Transaction.Rollback();
                IsOpenTransaction = false;
                if (OnDbExecuteException != null)
                {
                    var cmdText = CommandTextBuilder.ToString();
                    OnDbExecuteException(new DbExceptionInfo(exp, cmdText, new Parameter(string.Empty, null)));
                    CommandTextBuilder = null;
                }
                throw exp;
            }
            catch (Exception exp)
            {
                Transaction.Rollback();
                IsOpenTransaction = false;
                if (OnDbExecuteException != null)
                {
                    var cmdText = CommandTextBuilder.ToString();
                    OnDbExecuteException(new DbExceptionInfo(exp, cmdText, new Parameter(string.Empty, null)));
                    CommandTextBuilder = null;
                }
                throw exp;
            }
        }

        #endregion -- Transaction()

        #region -- Dispose()

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (Transaction != null)
            {
                if (Transaction.Connection != null)
                {
                    Transaction.Rollback();
                }
                Transaction.Dispose();
            }
            CloseConnection(CurrentConnection);
        }

        /// <summary>
        ///
        /// </summary>
        internal void CloseConnection(IDbConnection connection)
        {
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
                Command.Dispose();
                connection.Dispose();
            }
        }

        #endregion -- Dispose()
    }
}