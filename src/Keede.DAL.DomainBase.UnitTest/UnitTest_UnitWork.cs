using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Keede.DAL.DDD.Unitwork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keede.DAL.RWSplitting;
using Keede.DAL.DDD.UnitTest.Models;
using Dapper;
using Dapper.Extension;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Tests;
using Keede.DAL.DDD.Repositories;

namespace Keede.DAL.DDD.UnitTest
{
    /// <summary>
    /// UnitTest_UnitWork 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest_UnitWork
    {
        public UnitTest_UnitWork()
        {
            //string[] readConnctions = { "Data Source=192.168.117.155;Initial Catalog=Test_Slaver1;User Id = sa;Password = !QAZ2wsx;" };
            string[] readConnctions = { "server=192.168.117.189;database=Group.WMS;user id=test;password=t#@!$%;min pool size=20;max pool size=1000;" };
            string writeConnction = "server=192.168.117.189;database=Group.WMS;user id=test;password=t#@!$%;min pool size=20;max pool size=1000;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);

            TypeMapper.Initialize("Keede.DAL.DDD.UnitTest.Models");
        }

        //        [TestMethod]
        //        public void TestReg()
        //        {
        //            var typess = AppDomain.CurrentDomain.GetAssemblies()
        //                .SelectMany(a => a.GetTypes().Where(d=>d.IsClass && !d.IsAbstract).Where(t =>t.GetInterfaces().Any(d=>d.IsGenericType && d.GetGenericTypeDefinition()==typeof(IRepository<>))));
        //
        //            foreach (var type in typess)
        //            {
        //                var interfaceType = type.GetInterfaces().FirstOrDefault(d => d.IsGenericType && d.GetGenericTypeDefinition() == typeof(IRepository<>));
        //                var entityType = interfaceType.GenericTypeArguments.Where(i=>!i.IsGenericParameter).FirstOrDefault();
        //
        //                Type generic = typeof(SqlServerRepository<>);
        //                Type[] typeArgs = { entityType };
        //                Type realType = generic.MakeGenericType(typeArgs);
        //                ConstructorInfo constructor;
        //
        //
        //            }
        //
        //
        //            if (_constructorDic.TryGetValue(realType.FullName, out constructor))
        //                return constructor;
        //            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(ent => !ent.GlobalAssemblyCache && ent.FullName.ToLower().Contains("keede")))
        //            {
        //                foreach (var type in assembly.GetTypes().Where(type => realType.IsAssignableFrom(type)))//assembly.GetTypes().Where(ent => ent.IsClass).Where(ent => ent.BaseType == realType || (ent.BaseType != null && ent.BaseType.BaseType == realType)))
        //                {
        //                    constructor = type.GetConstructor(new Type[0]);
        //                    _constructorDic.AddOrUpdate(realType.FullName, constructor, (key, existed) => constructor);
        //                    return constructor;
        //                }
        //            }
        //
        //
        //
        //        }

        [TestMethod]
        public void TestExpression()
        {
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                CustomersEntity ce=new CustomersEntity();
                ce.DD=new Guid();
//                unitOfWork.RegisterModified<News>(ct => ct.Test1 == ce.DD, new { Title = "afterUpdateTitle1112" });

                ////Expression<Func<News, bool>> modifyQueryExpression = ct => ct.GId == 10000 && ct.Title == "updateTitle";
                ////unitOfWork.RegisterModified(modifyQueryExpression, new { Title = "afterUpdateTitle" });
                //update News set Title = 'afterUpdateTitle' where GID=10000 and Title='updateTitle'

                //Expression<Func<News, bool>> removeQueryExpression = ct => ct.GId == 10000 && ct.Title == "removeTitle";
                //unitOfWork.RegisterRemoved(removeQueryExpression);
                ////delete News where GID=10000 and Title='removeTitle'

                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestTryLockEntityObject()
        {
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                unitOfWork.BeginTransaction();
                var repository = new NewsRepository();
                var dynParms1 = new DynamicParameters();
                repository.SetDbTransaction(unitOfWork.DbTransaction);

                unitOfWork.TryLockEntityObject<News>(1,2);

                dynParms1.Add("@id", 2);
                var news2 = repository.Get("select * from news where Gid=@id", dynParms1);
                news2.Title = "123";
                repository.Save(news2);

                unitOfWork.Rollback();

                //NeNewsMultiIdws new1 = new NeNewsMultiIdws();
                //new1.Id = 1;
                //new1.GId = 1;

                //if (!unitOfWork.TryLockEntityObject(3, new1))
                //{
                //    return;
                //}

                unitOfWork.Commit();
            }
        }

        [TestMethod]
        public void TestCustomRepository()
        {
            //ICustomRepository custom = new CustomRepository();
            var news = new News{ Id=14,Title="title"+DateTime.Now};

            IUnitOfWork unitOfWork = new SqlServerUnitOfWork();
            unitOfWork.RegisterAdded(news);
            unitOfWork.Commit();
        }

        [TestMethod]
        public void TestUnitWrokSelectAndUpdate()
        {
            NewsRepository repository = new NewsRepository();

            var news = new News();
            news.Id = 998111;
            news.Title = "UnitWrokTitle998";

            IUnitOfWork unitOfWork = new SqlServerUnitOfWork();
            unitOfWork.RegisterAdded(news);
            //unitOfWork.RegisterRemoved(news);

            //var person = new Person();
            //person.Name = "name";
            //person.Id = new Guid("9E8D004F-21F6-432C-B1D5-DA5C01CA60ee");
            //unitOfWork.RegisterAdded(person);

            unitOfWork.Commit();

            //var news1 = repository.GetById(1);
            //if (news1 == null) return;
            //news1.Title = "UnitWrokTitle1";
            //unitOfWork.RegisterModified(news1);

        }

        [TestMethod]
        public void TestUnitWrokSelectAndUpdateNews()
        {

            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
            {
                unitOfWork.BeginTransaction();
                NewsRepository repository = new NewsRepository();//.SetDbTransaction(unitOfWork.DbTransaction);
                repository.SetDbTransaction(unitOfWork.DbTransaction);
                //if (!unitOfWork.TryLockEntityObject<News>(3, 1))
                //{
                //    return;
                //}

#if DEBUG
                Trace.WriteLine($"Connection > {unitOfWork.DbConnection.GetHashCode()}");
#endif

                var news1 = repository.GetById(1);
                if (news1 == null) return;

                var news2 = repository.GetById(2);
                if (news2 == null) return;
                //unitOfWork.BeginTransaction();//开启事务

                news1.Title= "UnitWrokTitle1211";
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

                Assert.IsNotNull(news2);
                Assert.IsTrue(list3.Count > 0);
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
            using (IUnitOfWork unitOfWork = new SqlServerUnitOfWork())
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

        [TestMethod]
        public void GetIdTest()
        {
            var box1=new Box();
            box1.BoxName = "1";
            var box2=new Box();
            box2.BoxName = "1";

            var id1 = EntityAttributeUtil.GetId(box1);
            var id2= EntityAttributeUtil.GetId(box1);

            var new1=new News();
            new1.Id = 1;
            new1.Title = "1";

            var new2=new News();
            new2.Id = 1;
            new2.Title = "1";
            var id3 = EntityAttributeUtil.GetId(new1);
            var id4 = EntityAttributeUtil.GetId(new2);
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
