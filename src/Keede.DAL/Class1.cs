using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace Keede.DAL
{
    class Sql
    {

        public void QueryFirst()
        {
            var conn= Database.GetDbConnection();
            conn.QueryFirst
        }
    }
}
