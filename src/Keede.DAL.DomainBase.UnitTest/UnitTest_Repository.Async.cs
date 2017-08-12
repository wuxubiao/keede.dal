using System;
using System.Collections.Generic;
using Dapper;
using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest;
using Keede.DAL.DDD.UnitTest.Models;
using Keede.DAL.RWSplitting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Keede.RepositoriesTests
{
    /// <summary>
    /// UnitTest_UnitWork 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestRepositoryAsync
    {
        public UnitTestRepositoryAsync()
        {
            //string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver1;User Id = sa;Password = !QAZ2wsx;" };
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;" };
            string writeConnction = "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB02", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod]
        public void TestAddAsync()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();
                news1.GId = 110;
                news1.Title = "title110";
                news1.Content = DateTime.Now.ToString();
                var result = repository.AddAsync(news1).Result;

                news1.GId = 111;
                news1.Title = "title111";
                news1.Content = DateTime.Now.ToString();
                var result2 = repository.AddAsync(news1).Result;

                Assert.IsTrue(result);
                Assert.IsTrue(result2);
            }
        }

        [TestMethod]
        public void TestBatchAddAsync()
        {
            using (var repository = new NewsRepository())
            {
                IList<News> list = new List<News>();

                var news1 = new News();
                news1.GId = 120;
                news1.Title = "title120";
                news1.Content = DateTime.Now.ToString();

                list.Add(news1);

                var news2 = new News();
                news2.GId = 121;
                news2.Title = "title121";
                news2.Content = DateTime.Now.ToString();
                list.Add(news2);

                var result = repository.BatchAddAsync(list).Result;

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestSaveAsync()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();

                news1.GId = 110;
                news1.Title = "title";
                news1.Content = "Content" + DateTime.Now;
                var result = repository.SaveAsync(news1).Result;

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestRepoRemoveAsync()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();
                news1.GId = 110;
                news1.Title = "title110";
                var result = repository.RemoveAsync(news1).Result;

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestRepoRemoveWhereSqlAsync()
        {
            using (var repository = new NewsRepository())
            {
                var result = repository.RemoveAsync("  Gid=111").Result;

                Assert.IsTrue(result > 0);
            }
        }

        [TestMethod]
        public void TestGetAsync()
        {
            using (var repository = new NewsRepository())
            {

                var dynParms1 = new DynamicParameters();
                dynParms1.Add("@id", 2);
                var news2 = repository.GetAsync("select * from news where Gid=@id", dynParms1).Result;

                var news1 = repository.GetAsync<News>("select * from news where Gid=@id", dynParms1).Result;

                var news3 = repository.GetByIdAsync(2).Result;

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
                Assert.IsNotNull(news3);
            }
        }

        [TestMethod]
        public void TestGetListAsync()
        {
            using (var repository = new NewsRepository())
            {
                var dynParms2 = new DynamicParameters();
                dynParms2.Add("@num", 5);
                var list2 = repository.GetListAsync<News>("select * from news where Gid>@num", dynParms2).Result;
                var list3 = repository.GetListAsync<News>("select * from news where Gid>5").Result;

                Assert.IsTrue(list2.Count > 0);
                Assert.IsTrue(list3.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetPagedListAsync()
        {
            using (var repository = new NewsRepository())
            {
                var list4 = repository.GetPagedListAsync("where Gid<=6", " order by Gid desc ", null, 2, 3).Result;
                var dynParms3 = new DynamicParameters();
                dynParms3.Add("@num", 6);
                var list5 = repository.GetPagedListAsync("where Gid<=@num", " Gid desc ", dynParms3, 2, 3).Result;

                var sql = "select * from News where Gid>2 order by Gid desc ";
                var list6 = repository.GetPagedListAsync<News>(sql, null, 1, 2).Result;

                Assert.IsTrue(list4.Items.Count > 0);
                Assert.IsTrue(list5.Items.Count > 0);
                Assert.IsTrue(list6.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetAllAsync()
        {
            using (var repository = new NewsRepository())
            {
                var list4 = repository.GetAllAsync().Result;

                Assert.IsTrue(list4.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetCountAsync()
        {
            var repository = new NewsRepository();
            var num = repository.GetCountAsync("select count(*) from news").Result;
            Assert.IsTrue(num > 0);
        }

        [TestMethod]
        public void TestRepoSelectAndUpdateAsync()
        {
            using (var repository = new NewsRepository())
            {
                var repository1 = new NewsRepository();

                var news1 = repository.GetByIdAsync(1).Result;
                news1.Title = "Title" + DateTime.Now;
                var result = repository.SaveAsync(news1).Result;

                var news2 = repository.GetByIdAsync(2).Result;

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestRepoINewsRepositoryAsync()
        {
            INewsRepository newsRepository = new NewsRepository();
            newsRepository.TestAdd(1);

            IRepository<News> repository = new NewsRepository();
            News news = new News();
            news.GId = 13;

            Assert.IsTrue(repository.AddAsync(news).Result);
        }

        [TestMethod]
        public void TestRepoSelectAsync()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = repository.GetById(1);
                if (news1 == null) return;

                var dynParms1 = new DynamicParameters();
                dynParms1.Add("@id", 2);
                var news2 = repository.Get("select * from news where id=@id", dynParms1);
                var news3 = repository.Get("select * from news where id=2");

                var list1 = repository.GetAll();

                var dynParms2 = new DynamicParameters();
                dynParms2.Add("@num", 5);
                var list2 = repository.GetList<News>("select * from news where id>@num", dynParms2);
                var list3 = repository.GetList<News>("select * from news where id>5");

                var list4 = repository.GetPagedList("where id<=6", " order by id desc ", null, 2, 3);
                var dynParms3 = new DynamicParameters();
                dynParms3.Add("@num", 6);
                var list5 = repository.GetPagedList("where id<=@num", " id desc ", dynParms3, 2, 3);

                var sql = "select * from News where id>2 order by id desc ";
                var list6 = repository.GetPagedList<News>(sql, null, 1, 2);

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
                Assert.IsNotNull(news3);
                Assert.IsTrue(list1.Count > 0);
                Assert.IsTrue(list2.Count > 0);
                Assert.IsTrue(list3.Count > 0);
                Assert.IsTrue(list4.Items.Count > 0);
                Assert.IsTrue(list5.Items.Count > 0);
                Assert.IsTrue(list6.Count > 0);
            }
        }
    }
}