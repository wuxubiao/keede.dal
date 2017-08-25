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
    /// <summary>
    /// UnitTest_UnitWork 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestRepository
    {
        public UnitTestRepository()
        {
            //string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver1;User Id = sa;Password = !QAZ2wsx;" };
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;" };
            string writeConnction = "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);

            //TypeMapper.Initialize("Keede.DAL.DDD.UnitTest.Models");
            //TypeMapper.SetTypeMap(typeof(News));
            //TypeMapper.SetTypeMap(typeof(NewsCustom));
            
            //SqlMapper.SetTypeMap(typeof(News), new ColumnAttributeTypeMapper<News>());
        }

        [TestMethod]
        public void TestDateTime()
        {
            var date = DateTime.Now;
            var sql = new StringBuilder();
            sql.Append(date);
            var sqls = sql.ToString();

            var sqlstr = "";
            sqlstr += date;

        }

        [TestMethod]
        public void TestAdd()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();
                news1.GId = 10;
                news1.Title = "title10";
                news1.Content = DateTime.Now.ToString();
                var result = repository.Add(news1);

                news1.GId = 11;
                news1.Title = "title11";
                news1.Content = DateTime.Now.ToString();
                var result2 = repository.Add(news1);

                var customRepository = new NewsCustomRepository();
                var newsCustom = new NewsCustom();
                newsCustom.Title = "title2121";
                customRepository.Add(newsCustom);

                Assert.IsTrue(result);
                Assert.IsTrue(result2);
            }
        }

        [TestMethod]
        public void TestBatchAdd()
        {
            using (var repository = new NewsRepository())
            {
                IList<News> list = new List<News>();

                var news1 = new News();
                news1.GId = 220;
                news1.Title = "title220";

                list.Add(news1);

                var news2 = new News();
                news2.GId = 221;
                news2.Title = "title221";
                list.Add(news2);

                var result = repository.BatchAdd(list);
                
                //IList<NewsCustom> list2 = new List<NewsCustom>();

                //var customRepository = new NewsCustomRepository();
                //var newsCustom = new NewsCustom();
                //newsCustom.Title = "title21";
                //newsCustom.Content = "ss";
                //list2.Add(newsCustom);

                //var newsCustom2 = new NewsCustom();
                //newsCustom2.Title = "title21";
                //newsCustom2.Content = "ss";
                //list2.Add(newsCustom2);

                //var result1=customRepository.BatchAdd(list2);

                Assert.IsTrue(result);
                //Assert.IsTrue(result1);
            }

        }

        [TestMethod]
        public void TestSave()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();

                news1.GId = 10;
                news1.Title = "title";
                news1.Content = "Content"+DateTime.Now;
                var result = repository.Save(news1);

                var newsCustom = new NewsCustom();

                var customRepository = new NewsCustomRepository();
                newsCustom.Id = 9;
                newsCustom.Title = "title";
                newsCustom.Content = "Content" + DateTime.Now;
                var result1 = customRepository.Save(newsCustom);

                Assert.IsTrue(result);
                Assert.IsTrue(result1);
            }
        }

        [TestMethod]
        public void TestRepoRemove()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();
                news1.GId = 10;
                news1.Title = "title10";
                var result = repository.Remove(news1);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestRepoRemoveWhereSql()
        {
            using (var repository = new NewsRepository())
            {
                var result = repository.Remove("  Gid=11");

                Assert.IsTrue(result>0);
            }
        }

        [TestMethod]
        public void TestGet()
        {
            //var map = new CustomPropertyTypeMap(typeof(News),
            //    (type, columnName) => type.GetProperties().FirstOrDefault(prop => SqlMapperExtensions.GetDescriptionFromAttribute(prop) == columnName));
            //SqlMapper.SetTypeMap(typeof(News), map);
            //TypeMapper.SetTypeMap(typeof(News));

            using (var repository = new NewsRepository())
            {

                var dynParms1 = new DynamicParameters();
                dynParms1.Add("@id", 2);
                var news2 = repository.Get("select * from news where Gid=@id", dynParms1);

                var news1 = repository.Get<News>("select * from news where Gid=@id", dynParms1);

                var news3 = repository.GetById(2);

                var custom = new NewsCustomRepository();
                var cust = custom.GetById(1);
                var cust2 = custom.Get("select * from NewsCustom where id=@id", dynParms1);

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
                Assert.IsNotNull(news3);
            }
        }

        [TestMethod]
        public void TestGetList()
        {
            using (var repository = new NewsRepository())
            {
                var dynParms2 = new DynamicParameters();
                dynParms2.Add("@num", 5);
                var list2 = repository.GetList<News>("select * from news where Gid>@num", dynParms2);
                var list3 = repository.GetList<News>("select * from news where Gid>5");

                Assert.IsTrue(list2.Count > 0);
                Assert.IsTrue(list3.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetPagedList()
        {
            using (var repository = new NewsRepository())
            {
                var list4 = repository.GetPagedList("where Gid<=6", " order by Gid desc ", null, 2, 3);
                var dynParms3 = new DynamicParameters();
                dynParms3.Add("@num", 6);
                var list5 = repository.GetPagedList("where Gid<=@num", " Gid desc ", dynParms3, 2, 3);

                var sql = "select * from News where Gid>2 order by Gid desc ";
                var list6 = repository.GetPagedList<News>(sql, null, 1, 2);

                Assert.IsTrue(list4.Items.Count > 0);
                Assert.IsTrue(list5.Items.Count > 0);
                Assert.IsTrue(list6.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetAll()
        {
            using (var repository = new NewsRepository())
            {
                var list4 = repository.GetAll();

                Assert.IsTrue(list4.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetCount()
        {
            var repository = new NewsRepository();
            var num = repository.GetCount("select count(*) from news");
            Assert.IsTrue(num > 0);
        }

        [TestMethod]
        public void TestRepoSelectAndUpdate()
        {
            using (var repository = new NewsRepository())
            {
                var repository1 = new NewsRepository();

                var news1 = repository.GetById(1);
                news1.Title = "Title"+DateTime.Now;
                var result=repository.Save(news1);

                var news2 = repository.GetById(2);

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestRepoINewsRepository()
        {
            INewsRepository newsRepository = new NewsRepository();
            newsRepository.TestAdd(1);

            IRepository<News> repository = new NewsRepository();
            News news = new News();
            news.GId = 11;

            Assert.IsTrue(repository.Add(news));
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