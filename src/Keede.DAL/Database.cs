using System.Data;
using System.Data.SqlClient;
using Dapper;
using Config.Keede.Library;

namespace Keede.DAL
{
    public class Database
    {
        static Database()
        {
            var connectionString = ConfManager.GetAppsetting("");
        }

        public static IDbConnection GetDbConnection()
        {
            return new SqlConnection();
        }
    }
}