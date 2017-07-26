using System.Data;
using System.Data.Common;

namespace Keede.SQLHelper
{
    /// <summary>
    ///
    /// </summary>
    internal abstract class DbFactory
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="providerName"></param>
        private static DbProviderFactory GetProviderFactory(string providerName)
        {
            if (providerName == string.Empty)
            {
                throw new System.ApplicationException("ProviderName的配置是空，请检查数据库配置名称是否正确！");
            }
            return DbProviderFactories.GetFactory(providerName);
        }

        internal static IDbConnection CreateConnection(string providerName, string connectionString)
        {
            var connection = GetProviderFactory(providerName).CreateConnection();
            if (connection != null)
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    connection.ConnectionString = connectionString;
                    return connection;
                }
                throw new System.ApplicationException("ConnectionString的配置是空，请检查数据库配置名称是否正确！");
            }
            return null;
        }
    }
}