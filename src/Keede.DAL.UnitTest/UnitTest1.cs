using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.Conntion;
using Dapper;

namespace Keede.DAL.UnitTest
{
    [TestClass]
    public class UnitTest1
    {

        public UnitTest1()
        {
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver2;User Id = sa;Password = !QAZ2wsx;" };
            ConnectionContainer.AddDbConnections("DB01", "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;", readConnctions, EnumStrategyType.Loop);
        }


        [TestMethod]
        public void TestRead()
        {
            for (int i = 0; i < 1; i++)
            {

                Databases db = new Databases();

                using (var connection = db.GetDbConnection())
                {
                    connection.Open();
                    var result = connection.Query<News>("SELECT TOP 1000 [Id],[Level],[Content],[CreateDate]FROM[Test_Master].[dbo].[Logs]");
                    var b = connection;
                }

                using (var connection = new Databases().GetDbConnection())
                {
                    connection.Open();
                    var result = connection.Query<News>("select top 10 * from news order by id");
                    int ii = 0;
                }
            }
        }

        [TestMethod]
        public void TestWrite()
        {
            for (int i = 0; i < 10; i++)
            {
                using (var connection = new Databases().GetDbConnection(false))
                {
                    connection.Execute("update news set title='中文1'");
                }
            }
        }
    }
}
