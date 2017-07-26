using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Keede.SQLHelper
{
    internal class Monitor
    {
        #region -- Run

        public static void Run(Action act, string connectionString, string commandText, params SqlParameter[] parameters)
        {
            if (Database.IsNeedMonitor)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                act();
                sw.Stop();
                OnMonitor(connectionString, new MonitorInfo(sw.ElapsedMilliseconds, commandText, Parameter.Get(parameters).ToArray()));
            }
        }

        public static TResult Run<TResult>(Func<TResult> func, string connectionString, string commandText, params SqlParameter[] parameters)
        {
            if (Database.IsNeedMonitor)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                TResult result = func();
                sw.Stop();
                OnMonitor(connectionString, new MonitorInfo(sw.ElapsedMilliseconds, commandText, Parameter.Get(parameters).ToArray()));
                return result;
            }
            return func();
        }

        #endregion -- Run

        #region -- Run

        public static TResult Run<TResult>(Func<TResult> func, string connectionString, string commandText, params Parameter[] parameters)
        {
            if (Database.IsNeedMonitor)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                TResult result = func();
                sw.Stop();
                OnMonitor(connectionString, new MonitorInfo(sw.ElapsedMilliseconds, commandText, parameters));
                return result;
            }
            return func();
        }

        #endregion -- Run

        #region -- Monitor

        private static void OnMonitor(string connectionString, MonitorInfo info)
        {
            var pageUrl = string.Empty;
            var context = System.Web.HttpContext.Current;
            if (context != null)
            {
                var url = context.Request.Url.AbsolutePath;
                pageUrl = url.Substring(url.LastIndexOf('/') + 1);
            }
            ThreadPool.QueueUserWorkItem(obj =>
            {
                IDbConnection conn = DbFactory.CreateConnection("System.Data.SqlClient", connectionString);
                try
                {
                    var cmd1 = CreateCommand(conn, CommandType.Text, MonitorInfo.VerfityTableSQL, null);
                    var val = cmd1.ExecuteScalar();
                    cmd1.Dispose();
                    if (val.ToString() == "0")
                    {
                        var cmd2 = CreateCommand(conn, CommandType.Text, MonitorInfo.CreateTableSQL, null);
                        cmd2.ExecuteNonQuery();
                        cmd2.Dispose();
                    }
                    var parms = new[]
                        {
                            new Parameter("@RequestFrom",pageUrl),
                            new Parameter("@QueryString",info.CommandText),
                            new Parameter("@ParameterString",info.ParameterString),
                            new Parameter("@ExceptionMessage",info.Exception!=null? info.Exception.Message:string.Empty),
                            new Parameter("@TimeConsuming",info.TimeConsuming)
                        };
                    var cmd3 = CreateCommand(conn, CommandType.Text, MonitorInfo.InsertSQL, parms);
                    cmd3.ExecuteNonQuery();
                    cmd3.Dispose();
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        #endregion -- Monitor

        private static IDbCommand CreateCommand(IDbConnection connection, CommandType cmdType, string cmdText, IEnumerable<Parameter> cmdParms)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandType = cmdType;
            cmd.CommandText = cmdText;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (cmd.Connection == null)
            {
                cmd.Connection = connection;
            }
            if (cmdParms != null)
            {
                foreach (var p in cmdParms)
                {
                    if (p != null)
                    {
                        var parm = cmd.CreateParameter();
                        parm.ParameterName = p.Name;
                        parm.Value = p.Value ?? DBNull.Value;
                        parm.Direction = p.Direction;
                        cmd.Parameters.Add(parm);
                    }
                }
            }
            return cmd;
        }
    }
}