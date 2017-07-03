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
        public void TestRepoNews()
        {
            using (var repository = new NewsRepository().SetDbConnection(false))
            {
                var news1 = repository.GetById(1);
                if (news1 == null) return;

                news1.Title = "RepoTitle1";
                repository.Save(news1);
                var news2 = repository.GetById(2);
                if (news2 == null) return;
                Assert.IsNotNull(news1);
                Assert.IsNotNull(news2);
            }
        }

        [TestMethod]
        public void TestRepoPerson()
        {
            var id1 = Guid.Parse("9E8D004F-21F6-432C-B1D5-DA5C01CA60DE");
            var id2 = Guid.Parse("848D4D32-6962-404D-BDFC-E61F2094D76C");
            using (var repository = new PersonRepository().SetDbConnection(false))
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
