using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Keede.DAL.RWSplitting;

namespace Keede.DAL.Helper
{
    /// <summary>
    /// DbHelper 用于封装数据库链接,以及基本操作的访问。
    /// 此控件为Sql Server操作封装。
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    public class DbHelper : IDisposable
    {
        internal IDbConnection CurrentConnection { get; set; }

        internal IDbTransaction Transaction { get; set; }

        internal IDbCommand Command { get; set; }

        internal bool IsOpenTransaction { get; set; }

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
        /// <param name="exception"></param>
        internal DbHelper(DbExecuteException exception)
        {
            Init(exception);
        }

        private void Init(DbExecuteException exception)
        {
            OnDbExecuteException = exception;
        }

        /// <summary>
        /// 传入执行事务，执行数据库访问，并返回受影响的数据行数。
        /// uparameters 类型参数传递。
        /// </summary>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回int型数据类型值，指示被影响的数据行</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        internal int ExecuteNonQuery(string cmdText, params Parameter[] parameters)
        {
            IDbConnection conn = IsOpenTransaction ? CurrentConnection : (SqlStatement.IsRead(cmdText) ? Databases.GetSqlConnection() : Databases.GetSqlConnection(false));
            try
            {
                //var cmd = CreateCommand(conn, cmdType, cmdText, parameters);
                MakeCommandTextLog(cmdText);
                int val = conn.Execute(cmdText, parameters, Transaction); //cmd.ExecuteNonQuery();
                //cmd.Parameters.Clear();
                return val;
            }
            catch (SqlException exp)
            {
                OnDbExecuteException?.Invoke(new DbExceptionInfo(exp, cmdText, parameters));
                throw exp;
            }
            catch (InvalidOperationException exp)
            {
                OnDbExecuteException?.Invoke(new DbExceptionInfo(exp, cmdText, parameters));
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
        /// 返回数据库查询结果,设定超时时间,用于耗时的查询
        /// </summary>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回SqlDataReader类查询结果</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        internal IDataReader ExecuteReader(string cmdText, params Parameter[] parameters)
        {
            try
            {
                IDbConnection conn = IsOpenTransaction ? CurrentConnection : Databases.GetSqlConnection();
                MakeCommandTextLog(cmdText);
                var reader = conn.ExecuteReader(cmdText, parameters, Transaction);

                //var cmd = CreateCommand(conn, cmdType, cmdText, parameters);
                //var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //cmd.Parameters.Clear();
                return reader;
            }
            catch (SqlException exp)
            {
                OnDbExecuteException?.Invoke(new DbExceptionInfo(exp, cmdText, parameters));
                throw exp;
            }
        }

        /// <summary>
        /// 传入现有数据库连接，执行查询，并返回查询所返回的结果集中第一行的第一列。
        /// using the provided parameters.
        /// </summary>
        /// <param name="cmdType">数据库执行模式</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="parameters">运行参数传递</param>
        /// <returns>返回object类查询结果</returns>
        [Obsolete("This function is obsolete,don't use it in new project")]
        internal object ExecuteScalar(string cmdText, params Parameter[] parameters)
        {
            IDbConnection conn = IsOpenTransaction ? CurrentConnection : (SqlStatement.IsRead(cmdText) ? Databases.GetSqlConnection() : Databases.GetSqlConnection(false));
            try
            {
                MakeCommandTextLog(cmdText);
                object val = conn.ExecuteScalar(cmdText, parameters, Transaction);

                //var cmd = CreateCommand(conn, cmdType, cmdText, parameters);
                //object val = cmd.ExecuteScalar();
                //cmd.Parameters.Clear();
                return val;
            }
            catch (SqlException exp)
            {
                OnDbExecuteException?.Invoke(new DbExceptionInfo(exp, cmdText, parameters));
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

        private void MakeCommandTextLog(string cmdText)
        {
            if (IsOpenTransaction && Transaction != null)
            {
                if (CommandTextBuilder == null)
                {
                    CommandTextBuilder = new StringBuilder();
                }
                CommandTextBuilder.Append(cmdText + " || ");
            }
        }

        /// <summary>
        /// 数据库指令执行模块
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cmdType">执行模式说明</param>
        /// <param name="cmdText">指令字符串</param>
        /// <param name="cmdParms">存储过程参数</param>
        //internal IDbCommand CreateCommand(IDbConnection connection, CommandType cmdType, string cmdText, IEnumerable<Parameter> cmdParms)
        //{
        //    Command = connection.CreateCommand();
        //    Command.CommandType = cmdType;
        //    Command.CommandText = cmdText;
        //    if (connection.State != ConnectionState.Open)
        //    {
        //        connection.Open();
        //    }
        //    if (Command.Connection == null)
        //    {
        //        Command.Connection = connection;
        //    }
        //    if (IsOpenTransaction && Transaction != null)
        //    {
        //        Command.Transaction = Transaction;
        //        if (CommandTextBuilder == null)
        //        {
        //            CommandTextBuilder = new StringBuilder();
        //        }
        //        CommandTextBuilder.Append(cmdText + " || ");
        //    }
        //    if (cmdParms != null)
        //    {
        //        foreach (var p in cmdParms)
        //        {
        //            if (p != null)
        //            {
        //                var parm = Command.CreateParameter();
        //                parm.ParameterName = p.Name;
        //                parm.Value = p.Value ?? DBNull.Value;
        //                parm.Direction = p.Direction;
        //                Command.Parameters.Add(parm);
        //            }
        //        }
        //    }
        //    return Command;
        //}

        #region -- Transaction()

        /// <summary>
        ///
        /// </summary>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.Unspecified);
        }

        /// <summary>
        ///
        /// </summary>
        [Obsolete("This function is obsolete,don't use it in new project")]
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (CurrentConnection == null)
            {
                CurrentConnection = Databases.GetDbConnection(false);
            }
            if (CurrentConnection != null)
            {
                if (CurrentConnection.State != ConnectionState.Open)
                {
                    CurrentConnection.Open();
                }
                Transaction = CurrentConnection.BeginTransaction(isolationLevel);
            }
            IsOpenTransaction = true;
        }

        /// <summary>
        ///
        /// </summary>
        [Obsolete("This function is obsolete,don't use it in new project")]
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
        [Obsolete("This function is obsolete,don't use it in new project")]
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
        [Obsolete("This function is obsolete,don't use it in new project")]
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