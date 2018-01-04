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
                {"server=192.168.117.189;database=Group.ERP;user id=test;password=t#@!$%;min pool size=20;max pool size=1000;"};
            string writeConnction =
                "server=192.168.117.189;database=Group.ERP;user id=test;password=t#@!$%;min pool size=20;max pool size=1000;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod()]
        public void ExecuteScalarSPTest()
        {
            DateTime startTime=new DateTime(2016,02,24);
            DateTime endTime = new DateTime(2016,02,25);
            var parms = new[] {
                new SqlParameter("@OrderState",9),
                new SqlParameter("@ConsignTimeStart", startTime),
                new SqlParameter("@ConsignTimeEnd",endTime)
            };

            var result = SqlHelper.ExecuteScalarSP("DB01", false, CommandType.StoredProcedure, "P_CompanyGrossProfitForEveryDay_GoodsOrder", parms);
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
