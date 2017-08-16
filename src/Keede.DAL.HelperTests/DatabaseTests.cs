using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keede.DAL.RWSplitting;

namespace Keede.DAL.Helper.Tests
{
    [TestClass()]
    public class DatabaseTests
    {
        public DatabaseTests()
        {
            string[] readConnctions =
                {"Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;"};
            string writeConnction =
                "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod()]
        public void SelectTest()
        {
            var db = new Database("DB01");

            var para = new Parameter("id", 1);


            string sql = "select * from news where Gid>@id";
            var dr = db.Select<News>(true, sql, para);
        }

        [TestMethod()]
        public void SelectTestNull()
        {
            var db = new Database("DB01");

            var para = new Parameter("id", 1);


            string sql = "select * from news";
            var dr = db.Select<News>(true, sql);
        }

        [TestMethod()]
        public void ExecuteReaderTest()
        {

            var parms = new[]
            {
                new SqlParameter("@id", 1)

            };
            string sql = "select * from news where gid>@id";

            using (IDataReader dr = SqlHelper.ExecuteReader("DB01", true, sql, parms))
            {
                while (dr.Read())
                {
                    var i=dr[0];
                    var qi=dr[1];
                    var qyi=dr[2];

                }
            }
        }

        [TestMethod()]
        public void ExecuteNonQueryTest()
        {

            var parms = new[]
            {
                new SqlParameter("@id", 1)

            };
            string sql = "update news set title='"+DateTime.Now+"' where gid=@id";
            var i=SqlHelper.ExecuteNonQuery("DB01",false, sql, parms);
        }
    }
}
