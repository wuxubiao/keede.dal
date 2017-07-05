using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Keede.DAL.DomainBase.Unitwork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.RWSplitting;
using Keede.DAL.DomainBase.UnitTest.Models;
using Dapper;

namespace Keede.DAL.DomainBase.UnitTest
{
    /// <summary>
    /// UnitTest_UnitWork 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest_UnitWork
    {
        public UnitTest_UnitWork()
        {
            string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver1;User Id = sa;Password = !QAZ2wsx;" };
            string writeConnction = "Data Source=192.168.117.155;Initial Catalog=Test_Master;User Id = sa;Password = !QAZ2wsx;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
        }

        [TestMethod]
        public void TestUnitWrokSelectAndUpdateNews()
        {

            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                NewsRepository repository = new NewsRepository();//.SetDbTransaction(unitOfWork.DbTransaction);
                
                if (!unitOfWork.TryLockEntityObject<News>(3, 1))
                {
                    return;
                }

#if DEBUG
                Trace.WriteLine($"Connection > {unitOfWork.DbConnection.GetHashCode()}");
#endif

                var news1 = repository.GetById(1);
                if (news1 == null) return;

                var news2 = repository.GetById(2);
                if (news2 == null) return;
                //unitOfWork.BeginTransaction();//开启事务

                news1.Title= "UnitWrokTitle1";
                unitOfWork.RegisterModified(news1);
                /////////////////////////////////////////////////////////////////////////////


                news2.Title = "cba";
                unitOfWork.RegisterModified(news2);

                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestUnitWrokAdd()
        {
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork(false))
            {
                var repository = new NewsRepository().SetDbTransaction(unitOfWork.DbTransaction);

                var news1 = new News();
                news1.Id = 6;
                news1.Title = "UnitWrokTitle6";
                unitOfWork.RegisterAdded(news1);

                var news2 = new News();
                news2.Id = 7;
                news2.Title = "UnitWrokTitle7";
                unitOfWork.RegisterAdded(news2);

                var result=unitOfWork.Commit();
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestUnitWrokSelect()
        {
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork(false))
            {
                var repository = new NewsRepository().SetDbTransaction(unitOfWork.DbTransaction);

                var dynParms1 = new DynamicParameters();
                dynParms1.Add("@id", 2);
                var news2 = repository.Get("select * from news where id=@id", dynParms1);

                //var list1 = repository.GetAll();//GetAll不允许使用

                var list3 = repository.GetList<News>("select * from news where id>5");
                var list4 = repository.GetPagedList("where id<=6", " order by id desc ", null, 2, 3);

                Assert.IsNotNull(news2);
                Assert.IsTrue(list3.Count > 0);
                Assert.IsTrue(list4.Items.Count > 0);
            }
        }

        [TestMethod]
        public void TestUnitWrokRemove()
        {
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork(false))
            {
                var repository = new NewsRepository().SetDbTransaction(unitOfWork.DbTransaction);

                var news1 = new News();
                news1.Id = 6;
                unitOfWork.RegisterRemoved(news1);

                var news2 = new News();
                news2.Id = 7;
                unitOfWork.RegisterRemoved(news2);

                var result = unitOfWork.Commit();
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestUnitWrokPerson()
        {
            var id1 = Guid.Parse("9E8D004F-21F6-432C-B1D5-DA5C01CA60DE");
            var id2 = Guid.Parse("848D4D32-6962-404D-BDFC-E61F2094D76C");
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork(false))
            {
                var repository = new PersonRepository().SetDbTransaction(unitOfWork.DbTransaction);
                if (!unitOfWork.TryLockEntityObject<Person>(3, id1, id2))
                {
                    return;
                }

#if DEBUG
                Trace.WriteLine($"Connection > {unitOfWork.DbConnection.GetHashCode()}");
#endif

                var person1 = repository.GetById(id1);
                if (person1 == null) return;

                var person2 = repository.GetById(id2);
                if (person2 == null) return;

                //////unitOfWork.BeginTransaction();//开启事务


                person1.Name = "UnitWrokName1";
                unitOfWork.RegisterModified(person1);


                person2.Name = "UnitWrokName2";
                unitOfWork.RegisterModified(person2);

                unitOfWork.Commit();
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
