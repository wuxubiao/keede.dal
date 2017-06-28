using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Keede.DAL;

namespace Keede.DAL
{
    public class Databases
    {
        #region SqlConnection对象
        public IDbConnection GetDbConnection(bool isReadDb = true)
        {
            return CreateConnction(null, isReadDb);
        }

        public IDbConnection GetDbConnection(string dbName, bool isReadDb = true)
        {
            return CreateConnction(dbName.ToLower(), isReadDb);
        }
        
        private IDbConnection CreateConnction(string dbName = null, bool isReadDb = true)
        {
            var connectionStr = dbName == null ? ConnectionContainer.GetConnction(isReadDb) : ConnectionContainer.GetConnction(dbName, isReadDb);
            return new SqlConnection(connectionStr);
        }
        #endregion Connection

        #region SqlConnection连接字符串
        public string GetDbConnectionStr(bool isReadDb = true)
        {
            return CreateConnctionStr(null, isReadDb);
        }

        public string GetDbConnectionStr(string dbName, bool isReadDb = true)
        {
            return CreateConnctionStr(dbName.ToLower(), isReadDb);
        }

        private string CreateConnctionStr(string dbName = null, bool isReadDb = true)
        {
            return dbName == null ? ConnectionContainer.GetConnction(isReadDb) : ConnectionContainer.GetConnction(dbName, isReadDb);
        }
        #endregion SqlConnection连接字符串

        //#region IDisposable Members

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

        //#endregion
    }
}