using Config.Keede.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keede.DAL.Conntion
{
    public static class ConnectionContainer
    {

        static IList<ConnectionWrapper> _lists= new List<ConnectionWrapper>();

        public static void AddDbConnections(string dbName, string writeConnction, string[] readConnctions=null, EnumStrategyType strategyType= EnumStrategyType.Loop)
        {
            //if (_lists.First(f => f.DbName == dbName) != null)
            //    throw new Exception("数据库已存在");

            if (readConnctions==null)
            {
                _lists.Add(new ConnectionWrapper(dbName, writeConnction));
            }
            else
            {
                BaseStrategy strategy=null;

                switch (strategyType)
                {
                    case EnumStrategyType.Loop:
                        strategy = new LoopStrategy();
                        break;
                    case EnumStrategyType.Random:
                        strategy = new RondomStrategy();
                        break;
                    default:
                        strategy= new LoopStrategy();
                        break;
                }
                _lists.Add(new ConnectionWrapper(dbName, writeConnction,readConnctions, strategy));
            }
        }

        [Obsolete("仅供单元测试调用")]
        public static void ClearConnections()
        {
            if(_lists!=null) _lists.Clear();
        }

        internal static string GetConnction(string dbName, bool isReadDb = true)
        {
            var wapper = _lists.First(f => f.DbName == dbName);

            if(wapper==null) throw new Exception("数据库不存在，请检查配置");

            return isReadDb ? wapper.ReadConnction : wapper.WriteConnction;
        }

        internal static string GetConnction(bool isRead = true)
        {
            if (_lists.Count != 1) throw new Exception("配置了多个数据库，请指定数据库");

            var wapper = _lists.First();
            return isRead ? wapper.ReadConnction : wapper.WriteConnction;
        }
    }
}
