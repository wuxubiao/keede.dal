using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
#if COREFX
using DataException = System.InvalidOperationException;
#else
using System.Threading;
#endif

namespace Dapper.Extension
{
    public class SqlCeServerAdapter : ISqlAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="tableName"></param>
        /// <param name="columnList"></param>
        /// <param name="parameterList"></param>
        /// <param name="keyProperties"></param>
        /// <param name="entityToInsert"></param>
        /// <returns></returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
        {
            var cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
            connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            var r = connection.Query("select @@IDENTITY id", transaction: transaction, commandTimeout: commandTimeout).ToList();

            if (r.First().id == null) return 0;
            var id = (int)r.First().id;

            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (!propertyInfos.Any()) return id;

            var idProperty = propertyInfos.First();
            idProperty.SetValue(entityToInsert, Convert.ChangeType(id, idProperty.PropertyType), null);

            return id;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="columnName"></param>
        public void AppendColumnName(StringBuilder sb, string columnName)
        {
            sb.AppendFormat("[{0}]", columnName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="columnName"></param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
        {
            sb.AppendFormat("[{0}] = @{1}", columnName, columnName);
        }
    }
}
