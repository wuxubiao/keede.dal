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

        [Obsolete("务必在应用启动时调用ConnectionContainer.AddDbConnections进行数据库配置初始化")]
        public IDbConnection GetDbConnection(bool isReadDb = true)
        {
            return CreateConnction(null, isReadDb);
        }

        [Obsolete("务必在应用启动时调用ConnectionContainer.AddDbConnections进行数据库配置初始化")]
        public IDbConnection GetDbConnection(string dbName, bool isReadDb = true)
        {
            return CreateConnction(dbName, isReadDb);
        }

        private IDbConnection CreateConnction(string dbName = null, bool isReadDb = true)
        {
            var connectionStr = dbName == null ? ConnectionContainer.GetConnction(isReadDb) : ConnectionContainer.GetConnction(dbName, isReadDb);
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
            Dispose(true);
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