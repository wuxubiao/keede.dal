using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keede.DAL.RWSplitting
{
    public class ConnectionWrapper
    {
        readonly BaseStrategy _strategy;
        readonly string[] _readConnections;

        internal string DbName { get; }

        internal string ReadConnction
        {
            get
            {
                if (_readConnections.Length != 0) return _strategy.GetConnctionStr(_readConnections);

                log4net.LogManager.GetLogger(typeof(BaseStrategy)).Warn($"警告：配置的从库节点数为0，尝试获取从库连接字符串失败，直接返回主库节点连接字符串：{WriteConnction}");
                return WriteConnction;
            }
        }

        public string WriteConnction { get; }

        internal ConnectionWrapper(string dbName, string writeConnction, string[] readConnctions, BaseStrategy strategy)
        {
            DbName = dbName;
            WriteConnction = writeConnction;
            _readConnections = readConnctions;
            _strategy = strategy;
        }

        internal ConnectionWrapper(string dbName, string writeConnction)
        {
            DbName = dbName;
            WriteConnction = writeConnction;
            _readConnections = new string[] { };
            _strategy = null;
        }
    }
}
