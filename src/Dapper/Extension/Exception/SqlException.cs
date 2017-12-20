using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
    public class SqlException : Exception
    {
        public SqlException() { }
        public SqlException(string message) 
            : base(message) { }

        public SqlException(string commandText, Exception inner)
            : base(inner.Message + "SQL语句:" + commandText, inner)
        {
            CommandText = commandText;
        }
        //public SqlException(string commandText)
        //{
        //    CommandText = commandText;
        //}

        public string CommandText { get; private set; }

    }
}
