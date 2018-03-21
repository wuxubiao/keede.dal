using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper.Extension;

namespace Dapper.Extensions.Tests
{

    [TestClass]
    public class WhereExpressionTest
    {
        [TestMethod]
        public void SimpleWhereTrueTest1()
        {
            //Expression<Func<CustomersEntity, bool>> queryExp1 = ct => 1==1 && ct.CustomerCity == "B-City";
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct =>  true && ct.TestBool==true && false && ct.TestBool==false && ct.TestBool == true && ct.CustomerCity == "B-City" && ct.CustomerID==1;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            //Expression<Func<CustomersEntity, bool>> queryExp11 = ct => true;
            //string sql11 = translate.Translate(queryExp11);

            //Assert.Equals(sql, "CustomerID < 50 AND CustomerCity = 'B-City'");
        }

        [TestMethod]
        public void SimpleWhereTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID < 50 && ct.CustomerCity == "B-City";

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID < 50 AND CustomerCity = 'B-City'");

        }

        [TestMethod]
        public void SimpleWhereTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID == 1 &&
                                                            (ct.CustomerCity == "B-City" || ct.CustomerNumber == "0000");

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID = 1 AND (CustomerCity = 'B-City' OR CustomerNumber = '0000')");

        }

        [TestMethod]
        public void SimpleWhereMethodTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && (SQLMethod.IsNotNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID <= 50 AND CustomerCity is Not NULL");

        }


        [TestMethod]
        public void SimpleWhereMethodTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID <= 50 AND CustomerCity is NULL");

        }

        [TestMethod]
        public void WhereMethodForParametersTest1()
        {
            TestValueTypeParam1(new Guid());
        }

        private void TestValueTypeParam1(Guid id)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.DD == id && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID <= 50 AND CustomerCity is NULL");
        }


        [TestMethod]
        public void WhereMethodForParametersTest()
        {
            TestValueTypeParam(50);
        }

        private void TestValueTypeParam(int id)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= id ;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID <= 50 AND CustomerCity is NULL");
        }

        [TestMethod]
        public void WhereMethodForParametersTest2()
        {
            TestEntityParam(new TestValue { Id = 50 });
        }

        private void TestEntityParam(TestValue v)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= v.Id && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID <= 50 AND CustomerCity is NULL");
        }

        [TestMethod]
        public void WhereMethodForParametersTest21()
        {
            TestEntityParam1(new TestValue { DD = new Guid() });
        }

        private void TestEntityParam1(TestValue v)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.DD == v.DD && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID <= 50 AND CustomerCity is NULL");
        }

        [TestMethod]
        public void SimpleWhereMethodTest3()
        {
            //IEnumerable<int> ids = new List<int>() { 40, 50 };//通过测试
            //int[] ids = new[] {40, 50};//通过测试
            List<int> ids = new List<int>() { 40, 50 };//通过测试

            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ids.Contains(ct.CustomerID) && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equals(sql, "CustomerID In (40,50) AND CustomerCity is NULL");

        }

        [TestMethod]
        public void LikeTest()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerName.Contains("X");
            Expression<Func<CustomersEntity, bool>> queryExp2 = ct => ct.CustomerName.StartsWith("X");
            Expression<Func<CustomersEntity, bool>> queryExp3 = ct => ct.CustomerName.EndsWith("X");
            var translate = new SqlTranslateFormater();
            string sql1 = translate.Translate(queryExp1);//"CustomerName like '%X%"
            string sql2 = translate.Translate(queryExp2);//"CustomerName like 'X%"
            string sql3 = translate.Translate(queryExp3);//"CustomerName like '%X"
        }
    }

    public class TestValue
    {
        public int Id { set; get; }
        public Guid DD { set; get; }

    }


}
