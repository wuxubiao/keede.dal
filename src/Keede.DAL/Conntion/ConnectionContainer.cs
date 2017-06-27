using Config.Keede.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keede.DAL.Conntion
{
    public static class ConnectionContainer
    {

        static IList<ConnectionWrapper> _lists;
        const string DAL_Connection_Strings = "DAL_Connection_Strings";

        public static void AddDbConnections(string dbName, string writeConnction, string[] readConnctions, EnumStrategyType strategyType)
        {

        }

       

        internal static string GetConnction(string dbName, bool isReadDb = true)
        {
            var wapper = _lists.First(f => f.DbName == dbName);
            return isReadDb ? wapper.ReadConnction : wapper.WriteConnction;
        }

        internal static string GetConnction(bool isRead = true)
        {
            if (_lists.Count != 1) throw new Exception("");

            var wapper = _lists.First();
            return isRead ? wapper.ReadConnction : wapper.WriteConnction;
        }
    }
}
