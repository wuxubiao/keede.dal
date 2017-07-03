using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.RWSplitting;
using Dapper;

namespace Keede.DAL.RWSplitting.UnitTest
{
    [TestClass]
    public class DatabasesTest
    {
        public DatabasesTest()
        {
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver2;User Id = sa;Password = !QAZ2wsx;" };
            string writeConnction = "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod]
        public void TestRead()
        {
            for (int i = 0; i < 1; i++)
            {

                using (var connection = Databases.GetDbConnection())
                {
                    connection.Open();
                    var result = connection.Query<News>("SELECT TOP 1000 * FROM Logs");
                }
            }
        }

        [TestMethod]
        public void TestWrite()
        {
            for (int i = 0; i < 10; i++)
            {
                using (var connection =  Databases.GetDbConnection(false))
                {
                    connection.Execute("update news set title='中文1'");
                }
            }
        }

        [TestMethod]
        public void TestMultiDB()
        {
            string[] readConnctions = { "Data Source=192.168.152.53;Initial Catalog=news;User Id=sa;Password = !QAZ2wsx;" };
            string writeConnction = "Data Source=192.168.152.52;Initial Catalog=news;User Id=sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB2", writeConnction, readConnctions, EnumStrategyType.Loop);

            using (var connection = Databases.GetDbConnection("db2"))
            {
                connection.Open();
                var result = connection.Query<News>("select top 10 * from news order by id");
            }
        }

        [TestMethod()]
        public void GetDbConnectionStrTest()
        {
            var connstr =  Databases.GetDbConnectionStr();
            var connstr2 =  Databases.GetDbConnectionStr(false);
        }
    }
}
