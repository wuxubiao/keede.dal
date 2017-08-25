using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest;
using Keede.DAL.DDD.UnitTest.Models;
using Keede.DAL.RWSplitting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Keede.RepositoriesTests
{
    [TestClass]
    public class UnitTest_WMS
    {
        public UnitTest_WMS()
        {
            string[] readConnctions = { "server=192.168.117.126;database=Group.WMS;user id=test;password=t#@!$%;" };
            string writeConnction = "server=192.168.117.126;database=Group.WMS;user id=test;password=t#@!$%;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod]
        public void TestAdd()
        {
            using (var repository = new NewsRepository())
            {
                var sql = @"select OB.[No], OB.WarehouseId, OB.StorageType, OB.HostingFilialeId, OB.SaleFilialeId, OB.CreateTime, 
OB.SourceNo, OB.SourceType, OB.[State], OB.FinishedTime, OB.ExpressId, OB.ExpressNo, OB.ApplyCancelTime, OB.IsExistCancelPool from OutGoodsBill OB WITH(NOLOCK) where  
WarehouseId = 'b5bcdf6e-95d5-4aee-9b19-6ee218255c05' AND IsExistCancelPool = 1 AND[State] IN('7', '8') ORDER by OB.CreateTime desc";
                repository.GetPagedList<News>(sql, null, 1, 10);

            }
        }

    }
}
