using System;
using System.Data.SqlClient;
using Framework.Common;

namespace Keede.SQLHelper
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class DbExceptionInfo
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="exception"></param>
        /// <param name="parameters"></param>
        public DbExceptionInfo(Exception exception, string commandText, params Parameter[] parameters)
        {
            ExceptionTime = DateTime.Now;
            CommandText = commandText;
            Exception = exception;
            ParameterString = Serialization.JsonSerialize(parameters);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="exception"></param>
        /// <param name="parameters"></param>
        public DbExceptionInfo(Exception exception, string commandText, params SqlParameter[] parameters)
        {
            ExceptionTime = DateTime.Now;
            CommandText = commandText;
            Exception = exception;
            ParameterString = Serialization.JsonSerialize(parameters);
        }

        /// <summary>
        ///
        /// </summary>
        public string ParameterString { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime ExceptionTime { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public string CommandText { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public Exception Exception { get; private set; }
    }
}