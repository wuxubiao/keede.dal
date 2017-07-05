using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
#if COREFX
using DataException = System.InvalidOperationException;
#else
using System.Threading;
#endif

namespace Dapper.Extension
{
    public interface ISqlAdapter
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
        int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="columnName"></param>
        void AppendColumnName(StringBuilder sb, string columnName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="columnName"></param>
        void AppendColumnNameEqualsValue(StringBuilder sb, string columnName);
    }
}
