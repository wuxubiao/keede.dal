using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;
using Dapper.Extensions.Tests;
using Entitys.Test;
using Keede.DAL.DDD.UnitTest;
using Keede.DAL.DDD.UnitTest.Models;
using Keede.DAL.DDD.Unitwork;
using Keede.DAL.RWSplitting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Entitys.Test.Models;
using Keede.DAL.DDD.Repositories;
using Repository.Test;

namespace Keede.RepositoriesTests
{
    [TestClass]
    public class UnitOfWorkTestNew
    {
        public UnitOfWorkTestNew()
        {
            string[] readConnctions = { "server=192.168.117.155;database=DAL;user id=sa;password=!QAZ2wsx;;min pool size=20;max pool size=1000;" };
            string writeConnction = "server=192.168.117.155;database=DAL;user id=sa;password=!QAZ2wsx;;min pool size=20;max pool size=1000;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);

            SqlServerUnitOfWork.InitRepository(typeof(News2).Assembly, typeof(News2Repository).Assembly);
        }

        [TestMethod]
        public void TestConstructorInfo()
        {
            var title = "Title " + DateTime.Now;

            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                var new21 = new News2 { Title = title };
                var new22 = new News2 { Title = title };
                var newsNotimpl1 = new NewsNotImpl { Title = title };
                var newsNotimp2l = new NewsNotImpl { Title = title };

                unitOfWork.RegisterAdded(new21);
                unitOfWork.RegisterAdded(new22);
                unitOfWork.RegisterAdded(newsNotimpl1);
                unitOfWork.RegisterAdded(newsNotimp2l);
                unitOfWork.Commit();
            }

            var news2Resp = new News2Repository();
            var newNoimplResp = new SqlServerRepository<NewsNotImpl>();

            var count = news2Resp.GetCount(t => t.Title == title);
            var count2 = newNoimplResp.GetCount(t => t.Title == title);

            Assert.IsTrue(count == 2 && count2 == 2);
        }

        [TestMethod]
        public void TestConstructorInfoNotImpl()
        {
            var title = "Title " + DateTime.Now;

            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                var new21 = new News3 { Id = Guid.NewGuid(), Title = title, CreateDateTime = DateTime.Now.AddYears(10) };
                var new22 = new News3 { Id = Guid.NewGuid(), Title = title, CreateDateTime = DateTime.Now.AddYears(10) };


                unitOfWork.RegisterAdded(new21);
                unitOfWork.RegisterAdded(new22);

                unitOfWork.Commit();
            }

            var newNoimplResp = new SqlServerRepository<News3>();

            var count2 = newNoimplResp.GetCount(t => t.Title == title);
            var count3 = newNoimplResp.GetCount(t => t.CreateDateTime > DateTime.Now);

            Assert.IsTrue(count2 == 2 && count3 > 1);
        }


        [TestMethod]
        public void TestConstructorInfoNotImplWithInterface()
        {
            var title = "Title " + DateTime.Now;

            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                var new21 = new News3 { Id = Guid.NewGuid(), Title = title, CreateDateTime = DateTime.Now.AddYears(10) };
                var new22 = new News3 { Id = Guid.NewGuid(), Title = title, CreateDateTime = DateTime.Now.AddYears(10) };


                unitOfWork.RegisterAdded(new21);
                unitOfWork.RegisterAdded(new22);

                unitOfWork.Commit();
            }

            IRepository<News3> newNoimplResp = new SqlServerRepository<News3>();

            var count2 = newNoimplResp.GetCount(t => t.Title == title);
            var count3 = newNoimplResp.GetCount(t => t.CreateDateTime > DateTime.Now);

            Assert.IsTrue(count2 == 2 && count3 > 1);
        }
    }
}
