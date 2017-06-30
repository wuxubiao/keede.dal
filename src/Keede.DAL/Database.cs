using System;
using System.Data;
using System.Data.SqlClient;

namespace Keede.DAL
{
    public static class Databases
    {
        #region IDbConnection对象
        public static IDbConnection GetDbConnection(bool isReadDb = true)
        {
            return CreateConnction(null, isReadDb);
        }

        public static IDbConnection GetDbConnection(string dbName, bool isReadDb = true)
        {
            return CreateConnction(dbName.ToLower(), isReadDb);
        }
        
        private static IDbConnection CreateConnction(string dbName = null, bool isReadDb = true)
        {
            var connectionStr = dbName == null ? ConnectionContainer.GetConnction(isReadDb) : ConnectionContainer.GetConnction(dbName, isReadDb);
            return new SqlConnection(connectionStr);
        }
        #endregion IDbConnection对象

        #region SqlConnection对象
        public static SqlConnection GetSqlConnection(bool isReadDb = true)
        {
            return CreateSqlConnection(null, isReadDb);
        }

        public static SqlConnection GetSqlConnection(string dbName, bool isReadDb = true)
        {
            return CreateSqlConnection(dbName.ToLower(), isReadDb);
        }

        private static SqlConnection CreateSqlConnection(string dbName = null, bool isReadDb = true)
        {
            var connectionStr = dbName == null ? ConnectionContainer.GetConnction(isReadDb) : ConnectionContainer.GetConnction(dbName, isReadDb);
            return new SqlConnection(connectionStr);
        }
        #endregion SqlConnection对象

        #region SqlConnection连接字符串
        public static string GetDbConnectionStr(bool isReadDb = true)
        {
            return CreateConnctionStr(null, isReadDb);
        }

        public static string GetDbConnectionStr(string dbName, bool isReadDb = true)
        {
            return CreateConnctionStr(dbName.ToLower(), isReadDb);
        }

        private static string CreateConnctionStr(string dbName = null, bool isReadDb = true)
        {
            return dbName == null ? ConnectionContainer.GetConnction(isReadDb) : ConnectionContainer.GetConnction(dbName, isReadDb);
        }
        #endregion SqlConnection连接字符串

        #region IDisposable Members

        //private bool _isDispose;

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //~Databases()
        //{
        //    Dispose(false);
        //}

        ///// <summary>
        ///// 释放链接
        ///// </summary>
        //public void Dispose(bool isDisposing)
        //{
        //    if (_isDispose) return;
        //    _isDispose = true;

        //    if (isDisposing && _connection.State== ConnectionState.Open)
        //        _connection.Close();
        //}

        #endregion
    }
}