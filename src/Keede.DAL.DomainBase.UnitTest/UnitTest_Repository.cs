using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using Dapper.Extension;
using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest;
using Keede.DAL.DDD.UnitTest.Models;
using Keede.DAL.RWSplitting;
using Keede.RepositoriesTests.Models;
using Keede.RepositoriesTests.Repositories;
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
//            string[] readConnctions = { "server=192.168.117.189;database=Group.WMS;user id=test;password=t#@!$%;min pool size=20;max pool size=1000;" };
//            string writeConnction = "server=192.168.117.189;database=Group.WMS;user id=test;password=t#@!$%;min pool size=20;max pool size=1000;";
//            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);

            //TypeMapper.Initialize("Keede.DAL.DDD.UnitTest.Models");
            ////TypeMapper.SetTypeMap(typeof(News));
            ////TypeMapper.SetTypeMap(typeof(NewsCustom));

            ////SqlMapper.SetTypeMap(typeof(News), new ColumnAttributeTypeMapper<News>());

//
            string[] readConnctions = { "server=192.168.117.155;database=DAL;user id=sa;password=!QAZ2wsx;;min pool size=20;max pool size=1000;" };
            string writeConnction = "server=192.168.117.155;database=DAL;user id=sa;password=!QAZ2wsx;;min pool size=20;max pool size=1000;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);

            TypeMapper.Initialize("Keede.DAL.DDD.UnitTest.Models");
        }

        [TestMethod]
        public void LikeTest()
        {
            Expression<Func<News, bool>> queryExp1 = ct => ct.Title.Contains("X");
            Expression<Func<News, bool>> queryExp2 = ct => ct.Title.StartsWith("X");
            Expression<Func<News, bool>> queryExp3 = ct => ct.Title.EndsWith("X");
            var translate = new SqlTranslateFormater();
            string sql1 = translate.Translate(queryExp1);
            string sql2 = translate.Translate(queryExp2);
            string sql3 = translate.Translate(queryExp3);

        }

        [TestMethod]
        public void TestRemoveExpression()
        {
            var repository = new NewsRepository();
            //Dictionary<string, Expression> _localExpressionDeletedCollection = new Dictionary<string, Expression>();


            //Expression<Func<News, bool>> queryExp2 = ct => ct.Id == 10000 && ct.Title == "removeTitle";
            //var tttt = queryExp2.Parameters[0].Type;
            //_localExpressionDeletedCollection.Add("s", queryExp2);

            //foreach (var modifiedData in _localExpressionDeletedCollection)
            //{
            //    var e = modifiedData.Value;
                
            //    var type = typeof(News);
            //    repository.RemoveExpression((Expression<Func<News, bool>>)e);

            //}

            Expression<Func<News, bool>> queryExp1 = ct => ct.Id == 220 && ct.Title == "afterUpdateTitle1111";

            var tt=queryExp1.Parameters[0].Type;

            var dic=new Dictionary<string,object>();
            dic.Add("Title", "afterUpdateTitle11111");
            dic.Add("Content", "afterUpdateContent11111");
            dic.Add("Test1", new Guid());
            repository.SaveExpression(queryExp1, dic);
            //repository.SaveExpression(queryExp1, new { Title = "afterUpdateTitle" });

            //repository.RemoveExpression(queryExp2);
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
                news1.Id = 10;
                news1.Title = "title10";
                news1.Content = DateTime.Now.ToString();
                var result = repository.Add(news1);

                news1.Id = 11;
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
                news1.Id = 9998;
                news1.Title = "title1" + DateTime.Now;
                news1.Content = DateTime.Now.ToString();
//                news1.Test1=new Guid();
                list.Add(news1);

                var news2 = new News();
                news2.Id = 9999;
                news2.Title = "title2" +DateTime.Now;
                news2.Content = DateTime.Now.ToString();
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
        public void TestBatchAddForNullAble()
        {

            using (var repository = new NewsRepository())
            {
                IList<News> list = new List<News>();

                var news1 = new News();
                news1.Title = null;
//                news1.Test1 = new Guid();
                news1.Content = DateTime.Now.ToString();
                list.Add(news1);

                var news2 = new News();
                news2.Title = "title2" + DateTime.Now;
                news2.Content = DateTime.Now.ToString();
                list.Add(news2);

                var result = repository.BatchAdd(list);


                Assert.IsTrue(result);
            }

        }

        [TestMethod]
        public void TestBatchAdd2()
        {
            var couponNoEntityList = new List<CouponNoEntity>
            {
                new CouponNoEntity
                {
                    No = 1008209,
                    CouponNo = "64ZTE4H",
                    PromotionID = new Guid("0D6B14DD-2CFD-46F5-AF48-3D989F258430"),
                    UseStartTime = DateTime.Parse("2017-01-23 00:00:00.000"),
                    UseEndTime = DateTime.Parse("2017-01-24 23:59:59.997"),
                    MemberID = Guid.NewGuid(),
                    BuildTime = DateTime.Parse("2017-01-22 15:44:02.223"),
                    IsUsed = false
                },
                new CouponNoEntity
                {
                    No = 1008210,
                    CouponNo = "5K9HSJ",
                    PromotionID = new Guid("0D6B14DD-2CFD-46F5-AF48-3D989F258430"),
                    UseStartTime = DateTime.Parse("2017-01-23 00:00:00.000"),
                    UseEndTime = DateTime.Parse("2017-01-24 23:59:59.997"),
                    MemberID = Guid.NewGuid(),
                    BuildTime = DateTime.Parse("2017-01-22 15:44:02.240"),
                    IsUsed = false
                }
            };

            var repository = new CouponNoRepository();
            repository.BatchAdd(couponNoEntityList);
        }

        [TestMethod]
        public void TestBatchAdd3()
        {
            var repo=new NewsRepository();
            repo.TestTestBatchAdd();
        }

        [TestMethod]
        public void TestBatchUpdate()
        {
            using (var repository = new NewsRepository())
            {
                IList<News> list = new List<News>();

                var news1 = new News();
                news1.Title = "title2111";
                news1.Id = 1;
                list.Add(news1);

                //var news2 = new News();
                //news2.Title = "title221";
                //list.Add(news2);

                var parameters = new[]
                {
                    new SqlParameter("@Id", SqlDbType.Int, 4, "Id"),
                    new SqlParameter("@Title", SqlDbType.NVarChar, 50, "Title"),
                };

                var result = repository.BatchUpdate(list,"update news set title=@Title where Id=@Id", parameters:parameters);

                var result2 = repository.BatchUpdate(list, "update news set title=@Title where Id=@Id", parameters: parameters);


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

                //Assert.IsTrue(result);
                //Assert.IsTrue(result1);
            }

        }

        [TestMethod]
        public void IsExistTest()
        {
            using (var repository = new NewsRepository())
            {
                var result1=repository.IsExist(new {Id = 100001, Title = "title2111" });
                var result2 = repository.IsExist("select top 1 1 from news where Id=@Id", new { Id = 100001});

            }
        }

        [TestMethod]
        public void TestSave()
        {
            using (var repository = new NewsRepository())
            {
                var news1 = new News();

                news1.Id = 10;
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
                news1.Id = 10;
                news1.Title = "title10";
                var result = repository.Remove(news1);

                Assert.IsTrue(result);
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
                var news11 = new News();
                news11.Id = 2;
                news11.Title = "321";
                var new5 = repository.Get(news11);


                var new4 = repository.Get(new {Id = 2,Title="321"});

                var dynParms1 = new DynamicParameters();
                dynParms1.Add("@id", 2);
                var news2 = repository.Get("select * from news where Id=@id", dynParms1);

                var news1 = repository.Get<News>("select * from news where Id=@id", dynParms1);

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
                var list2 = repository.GetList<News>("select * from news where Id>@num", dynParms2);
                var list3 = repository.GetList<News>("select * from news where Id>5");

                Assert.IsTrue(list2.Count > 0);
                Assert.IsTrue(list3.Count > 0);
            }
        }

        [TestMethod]
        public void TestGetListExpression()
        {
            using (var repository = new NewsRepository())
            {
                Expression<Func<News, bool>> queryExp1 = ct => ct.Id == 220 && ct.Title == "afterUpdateTitle1111";

                var list2 = repository.GetList(queryExp1);

                Assert.IsTrue(list2.Count > 0);

            }
        }

        [TestMethod]
        public void TestGetPagedList()
        {
            using (var repository = new NewsRepository())
            {

                var sql = "select * from News where Id>2 order by Id desc ";
                var list6 = repository.GetPagedList<News>(sql, null, 1, 2);

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
            news.Id = 11;

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

                var dynParms3 = new DynamicParameters();
                dynParms3.Add("@num", 6);

                var sql = "select * from News where id>2 order by id desc ";
                var list6= repository.GetPagedList<News>(sql, null, 1, 2);

                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
                Assert.IsNotNull(news3);
                Assert.IsTrue(list1.Count > 0);
                Assert.IsTrue(list2.Count > 0);
                Assert.IsTrue(list3.Count > 0);
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