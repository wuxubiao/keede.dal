﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keede.DAL
{
    public static class ConnectionContainer
    {

        static IList<ConnectionWrapper> _lists = new List<ConnectionWrapper>();

        public static void AddDbConnections(string dbName, string writeConnction, string[] readConnctions = null, EnumStrategyType strategyType = EnumStrategyType.Loop)
        {
            var ss = _lists.FirstOrDefault(f => f.DbName == dbName);
            if (_lists.Count > 0 && _lists.FirstOrDefault(f => f.DbName == dbName) != null) throw new Exception("dbName重复");

            if (readConnctions == null)
            {
                _lists.Add(new ConnectionWrapper(dbName.ToLower(), writeConnction));
            }
            else
            {
                BaseStrategy strategy = null;

                switch (strategyType)
                {
                    case EnumStrategyType.Loop:
                        strategy = new LoopStrategy();
                        break;
                    case EnumStrategyType.Random:
                        strategy = new RondomStrategy();
                        break;
                    default:
                        strategy = new LoopStrategy();
                        break;
                }
                _lists.Add(new ConnectionWrapper(dbName.ToLower(), writeConnction, readConnctions, strategy));
            }
        }

        internal static string GetConnction(string dbName, bool isReadDb = true)
        {
            var wapper = _lists.FirstOrDefault(f => f.DbName == dbName);

            if (wapper == null)
            {
                if (_lists.Count == 0) throw new Exception("数据库配置没有初始化，需在应用启动调用ConnectionContainer.AddDbConnections进行初始化");

                throw new Exception("数据库不存在，请检查配置");
            }

            return isReadDb ? wapper.ReadConnction : wapper.WriteConnction;
        }

        internal static string GetConnction(bool isRead = true)
        {
            if (_lists.Count != 1)
            {
                if (_lists.Count == 0) throw new Exception("数据库配置没有初始化，需在应用启动调用ConnectionContainer.AddDbConnections进行初始化");

                throw new Exception("配置了多个数据库，请指定数据库");
            }

            var wapper = _lists.First();
            return isRead ? wapper.ReadConnction : wapper.WriteConnction;
        }
    }
}
