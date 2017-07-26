using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Keede.SQLHelper;

namespace Framework.FastData
{
    /// <summary>
    /// 批量插入
    /// </summary>
    public class BatchExecute
    {
        /// <summary>
        /// 批量插入，手动映射表字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectString">连接字符串</param>
        /// <param name="data">数据源</param>
        /// <param name="tableName">插入的表名</param>
        /// <param name="mappings">Key是模型中的字段名，Value是对应数据表中的字段名</param>
        public static Int32 Insert<T>(string connectString, IEnumerable<T> data, string tableName, Dictionary<string, string> mappings)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var count = Insert<T>(transaction, data, tableName, mappings);
                        transaction.Commit();
                        return count;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 批量插入，手动映射表字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction"></param>
        /// <param name="data">数据源</param>
        /// <param name="tableName">插入的表名</param>
        /// <param name="mappings">Key是模型中的字段名，Value是对应数据表中的字段名</param>
        public static Int32 Insert<T>(SqlTransaction transaction, IEnumerable<T> data, string tableName, Dictionary<string, string> mappings)
        {
            SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction);
            sqlBulkCopy.BatchSize = data.Count();
            sqlBulkCopy.DestinationTableName = tableName;

            foreach (var mapping in mappings)
            {
                sqlBulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
            }

            sqlBulkCopy.BulkCopyTimeout = int.MaxValue;

            sqlBulkCopy.WriteToServer(data.ToDataTable());

            sqlBulkCopy.Close();
            return sqlBulkCopy.BatchSize;
        }

        /// <summary>
        /// 批量插入，自动映射表字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectString"></param>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        public static Int32 Insert<T>(string connectString, IEnumerable<T> data, string tableName)
        {
            var mappings = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToDictionary(ent => ent.Name, ent => ent.Name);
            return Insert(connectString, data, tableName, mappings);
        }

        /// <summary>
        /// 批量插入，自动映射表字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction"></param>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        public static Int32 Insert<T>(SqlTransaction transaction, IEnumerable<T> data, string tableName)
        {
            var mappings = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToDictionary(ent => ent.Name, ent => ent.Name);
            return Insert(transaction, data, tableName, mappings);
        }

        /// <summary> 
        /// 批量更新数据 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="data"></param>
        /// <param name="updateCommandText"></param>
        /// <param name="parameters"></param>
        public static int Update<T>(IDbTransaction transaction, IEnumerable<T> data, string updateCommandText,params SqlParameter[] parameters)
        {
            if (transaction == null)
            {
                throw new ApplicationException("transaction is null");
            }
            var cmd = new SqlCommand(updateCommandText, (SqlConnection)transaction.Connection,
                (SqlTransaction)transaction)
            { CommandTimeout = int.MaxValue };
            cmd.Parameters.AddRange(parameters);
            int result;
            using (var adapter = new SqlDataAdapter {UpdateCommand = cmd})
            {
                using (var table = data.ToDataTable())
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        if (dr.RowState == DataRowState.Unchanged)
                            dr.SetModified();
                    }
                    result = adapter.Update(table);
                    table.AcceptChanges();
                }
            }
            return result;
        }

        /// <summary> 
        /// 批量更新数据 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="data"></param>
        /// <param name="updateCommandText"></param>
        /// <param name="parameters"></param>
        public static int Update<T>(string connectionString, IEnumerable<T> data, string updateCommandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        return Update<T>(transaction, data, updateCommandText, parameters);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
