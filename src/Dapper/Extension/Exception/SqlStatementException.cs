using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
    /// <summary>
    /// 用于跟踪抛错的sql语句
    /// </summary>
    public class SqlStatementException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public SqlStatementException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public SqlStatementException(string message) 
            : base(message) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="inner"></param>
        public SqlStatementException(string commandText, object parameters, Exception inner)
            : base(inner.Message + " SQL语句:" + commandText + " 参数:" + Newtonsoft.Json.JsonConvert.SerializeObject(parameters), inner)
        {
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// SQL语句
        /// </summary>
        public string CommandText { get; private set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object Parameters { get; private set; }
    }
}
