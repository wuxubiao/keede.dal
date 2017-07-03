using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.RWSplitting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.RWSplitting.UnitTest
{
    [TestClass()]
    public class ConnectionContainerTests
    {
        [TestMethod()]
        public void AddDbConnectionsTest()
        {
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver2;User Id = sa;Password = !QAZ2wsx;" };
            ConnectionContainer.AddDbConnections("DB01", "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;", readConnctions, EnumStrategyType.Loop);

            ConnectionContainer.AddDbConnections("DB02", "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;", readConnctions, EnumStrategyType.Loop);
        }
    }
}