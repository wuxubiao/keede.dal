using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Config.Keede.Library;
using Keede.DAL.Conntion;

namespace Keede.DAL
{
    public class Databases : IDisposable
    {
        IDbConnection _connection;

        public IDbConnection GetDbConnection(bool isReadDb = true)
        {
            return CreateConnction(null, isReadDb);
        }

        public IDbConnection GetDbConnection(string dbName, bool isReadDb = true)
        {
            return CreateConnction(dbName, isReadDb);
        }

        private IDbConnection CreateConnction(string dbName = null, bool isReadDb = true)
        {
            var connectionStr = ConnectionContainer.GetConnction(dbName, isReadDb);
            _connection = new SqlConnection(connectionStr);
            return _connection;
        }

        #region IDisposable Members

        private bool _isDispose;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Databases()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放链接
        /// </summary>
        public void Dispose(bool isDisposing)
        {
            if (_isDispose) return;
            _isDispose = true;

            if (isDisposing)
                _connection.Close();
        }

        #endregion
    }
}