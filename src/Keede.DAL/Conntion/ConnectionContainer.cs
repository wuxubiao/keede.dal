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

        static ConnectionContainer()
        {
            ConfManager.ValueChanged += ConfManager_ValueChanged;

            //todo  list read from conf manager
        }

        private static void ConfManager_ValueChanged(object sender, Config.Keede.Common.Event.ConfigValueChangedEventArgs args)
        {
            if (args.Name == DAL_Connection_Strings)
            {

            }
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
