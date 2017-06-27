using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Keede.DAL
{
    public class SqlConnectionWrapper : IDisposable
    {
        /// <summary>
        /// 真正的数据库链接
        /// </summary>
        private readonly DbConnection _connection;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="connection">数据库链接</param>
        /// <param name="disposeConnection">是否释放链接</param>
        public SqlConnectionWrapper(DbConnection connection)
        {
            _connection = connection;
            _isDispose = false;
        }

        public DbConnection Connection
        {
            get { return _connection; }
        }

        #region IDisposable Members
        private Boolean _isDispose;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqlConnectionWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放链接
        /// </summary>
        public void Dispose(Boolean isDisposing)
        {
            if (_isDispose) return;
            _isDispose = true;

            if (isDisposing &&_connection.State== ConnectionState.Open)
                _connection.Close();
        }
        #endregion
    }
}
