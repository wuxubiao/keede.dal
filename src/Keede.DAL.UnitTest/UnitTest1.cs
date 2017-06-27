using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.Conntion;
using Dapper;

namespace Keede.DAL.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRead()
        {
            Init();
            for (int i = 0; i < 10; i++)
            {

                Databases db = new Databases();

                using (var connection = db.GetDbConnection())
                {

                    var result = connection.Query<News>("select top 10 * from news order by id");
                    var b = connection;
                }

                using (var connection = new Databases().GetDbConnection())
                {
                    var result = connection.Query<News>("select top 10 * from news order by id");
                    int ii = 0;
                }
            }
        }

        [TestMethod]
        public void TestWrite()
        {
            Init();
            for (int i = 0; i < 10; i++)
            {
                using (var connection = new Databases().GetDbConnection(false))
                {
                    connection.Execute("update news set title='中文1'");
                }
            }
        }

        private void Init()
        {
            ConnectionContainer.ClearConnections();
            string[] readConnctions = { "Data Source=tcp:192.168.152.52,1433;Initial Catalog=DB01;User ID=sa;Password=!QAZ2wsx;","Data Source=tcp:192.168.152.53,1433;Initial Catalog=DB01;User ID=sa;Password=!QAZ2wsx;" };
            ConnectionContainer.AddDbConnections("DB01", "Data Source=tcp:192.168.152.52,1433;Initial Catalog=DB01;User ID=sa;Password=!QAZ2wsx;",readConnctions,EnumStrategyType.Random);
        }
    }
}
