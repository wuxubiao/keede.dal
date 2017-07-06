using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using Keede.DAL.DomainBase.Unitwork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.RWSplitting;
using Keede.DAL.DomainBase.UnitTest.Models;
using Dapper;
using Keede.DAL.DomainBase.Repositories;

namespace Keede.DAL.DomainBase.UnitTest
{
    /// <summary>
    /// UnitTest_UnitWork 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest_Repository
    {
        public UnitTest_Repository()
        {
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver1;User Id = sa;Password = !QAZ2wsx;" };
            string writeConnction = "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod]
        public void TestRepoAdd()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();
                news1.Id = 4;
                news1.Title = "title4";
                var result = repository.Add(news1);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestSave()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();

                news1.Id = 4;
                news1.Title = "title4";
                news1.Content = "aaa";
                var result = repository.Save(news1);

                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void TestRepoRemove()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();
                news1.Id = 4;
                news1.Title = "title4";
                var result = repository.Remove(news1);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestGet()
        {
            using (var repository = new NewsRepository())
            {
                var dynParms1 = new DynamicParameters();
                dynParms1.Add("@id", 2);
                var news2 = repository.Get("select * from news where id=@id", dynParms1);

                var news1 = repository.Get<News>("select * from news where id=@id", dynParms1);

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
            }
        }


        [TestMethod]
        public void TestRepoSelectAndUpdate()
        {
            using (var repository = new NewsRepository())
            {
                var repository1 = new NewsRepository();
                var news3 = repository1.GetById(1,false);

                var news1 = repository.GetById(1);
                if (news1 == null) return;


                news1.Title = "RepoTitle11";
                repository.Save(news1);
                var news2 = repository.GetById(2);
                if (news2 == null) return;
                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
            }
        }

        [TestMethod]
        public void TestRepoINewsRepository()
        {
            //Keede.DAL.DomainBase.UnitTest.Models
            INewsRepository<News> news = new NewsRepository();
            news.TestAdd(1);
            IRepository<News> news2 = new NewsRepository();
            //news2.
        }

        [TestMethod]
        public void TestGetCount()
        {
            var repository = new NewsRepository();
            var num = repository.GetCount("select * from news where id>5");
            Assert.IsTrue(num > 0);
        }

        [TestMethod]
        public void TestRepoSelect()
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
                var list2=repository.GetList<News>("select * from news where id>@num", dynParms2);
                var list3= repository.GetList<News>("select * from news where id>5");

                var list4 = repository.GetPagedList("where id<=6"," order by id desc ",null,2,3);
                var dynParms3 = new DynamicParameters();
                dynParms3.Add("@num", 6);
                var list5 = repository.GetPagedList("where id<=@num", " id desc ", dynParms3, 2, 3);

                var sql = "select * from News where id>2 order by id desc ";
                var list6= repository.GetPagedList<News>(sql, null, 1, 2);

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



        [TestMethod]
        public void TestRepoRemoveWhereSql()
        {
            using (var repository = new NewsRepository())
            {
                var result = repository.Remove("  id=999");

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestRepoPerson()
        {
            var id1 = Guid.Parse("9E8D004F-21F6-432C-B1D5-DA5C01CA60DE");
            var id2 = Guid.Parse("848D4D32-6962-404D-BDFC-E61F2094D76C");
            using (var repository = new PersonRepository())
            {
                var person1 = repository.GetById(id1);
                if (person1 == null) return;

                person1.Name = "RepoName1";
                repository.Save(person1);
                var person2 = repository.GetById(id2);
                if (person2 == null) return;
                Assert.IsNotNull(person1);
                Assert.IsNotNull(person2);
            }
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
    }
}
