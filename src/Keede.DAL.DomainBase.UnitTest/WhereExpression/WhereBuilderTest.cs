using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;
using Dapper.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Keede.RepositoriesTests.WhereExpression
{
    [TestClass]
    public class WhereBuilderTest
    {
        [TestMethod]
        public void LikeTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerName.Contains("X") ;

            var builder = new SqlTranslateFormater();
            var sql = builder.Translate(queryExp1);

            Assert.AreEqual(sql, "([CustomerName] LIKE '%X%')");
        }

        [TestMethod]
        public void LikeTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerName.Contains("X") && ct.CustomerCity.Contains("Y") || ct.CustomerName.StartsWith("A") || ct.CustomerName.EndsWith("B");

            var builder = new SqlTranslateFormater();
            var sql = builder.Translate(queryExp1);

            Assert.AreEqual(sql, "(((([CustomerName] LIKE '%X%') AND ([CustomerCity] LIKE '%Y%')) OR ([CustomerName] LIKE 'A%')) OR ([CustomerName] LIKE '%B'))");
        }

        [TestMethod]
        public void KeyTest()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.Key == 1;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "([Key] = 1)");
        }

        [TestMethod]
        public void BoolTest()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => !ct.TestBool && ct.CustomerID == 2;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "((NOT ([TestBool] = 1)) AND ([CustomerID] = 2))");
        }

        [TestMethod]
        public void BoolTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.TestBool;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "([TestBool] = 1)");
        }

        [TestMethod]
        public void BoolTest3()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID == 2 && ct.TestBool ;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] = 2) AND ([TestBool] = 1))");
        }

        [TestMethod]
        public void BoolTest4()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.TestBool  && (ct.CustomerID>1 || true);

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([TestBool] = 1) AND (([CustomerID] > 1) OR (1=1)))");
        }

        [TestMethod]
        public void BoolTest5()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.TestBool==false && ct.TestBool || ct.TestBool ==true;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "((([TestBool] = 0) AND ([TestBool] = 1)) OR ([TestBool] = 1))");
        }

        [TestMethod]
        public void stringTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerCity == "B-City";

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "([CustomerCity] = 'B-City')");
        }

        [TestMethod]
        public void IntTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID == 50 && ct.CustomerID > 60;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] = 50) AND ([CustomerID] > 60))");
        }

        [TestMethod]
        public void SimpleWhereTrueTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => (ct.TestBool == false && ct.CustomerCity == "B-City" && ct.CustomerID == 1 )|| ct.TestBool == true;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(((([TestBool] = 0) AND ([CustomerCity] = 'B-City')) AND ([CustomerID] = 1)) OR ([TestBool] = 1))");
        }

        [TestMethod]
        public void SimpleWhereTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID < 50 && ct.CustomerCity == "B-City";

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] < 50) AND ([CustomerCity] = 'B-City'))");

        }

        [TestMethod]
        public void SimpleWhereTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID == 1 &&
                                                            (ct.CustomerCity == "B-City" || ct.CustomerNumber == "0000");
            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] = 1) AND (([CustomerCity] = 'B-City') OR ([CustomerNumber] = '0000')))");
        }

        [TestMethod]
        public void NullTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && ct.CustomerCity == null;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] <= 50) AND ([CustomerCity] IS NULL))");
        }

        [TestMethod]
        public void NullTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => SQLMethod.IsNull(ct.CustomerCity) && ct.CustomerID <= 50;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerCity] IS NULL) AND ([CustomerID] <= 50))");
        }

        [TestMethod]
        public void IsNotNullTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && ct.CustomerCity != null;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] <= 50) AND ([CustomerCity] <> NULL))");
        }

        [TestMethod]
        public void IsNotNullTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => SQLMethod.IsNotNull(ct.CustomerCity) && ct.CustomerID <= 50;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerCity] IS NOT NULL) AND ([CustomerID] <= 50))");
        }

        [TestMethod]
        public void GetDateTest()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CreateDateTime == SQLMethod.GetDate();

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "([CreateDateTime] = GETDATE())");
        }

        [TestMethod]
        public void SimpleWhereMethodTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && (SQLMethod.IsNotNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] <= 50) AND ([CustomerCity] IS NOT NULL))");
        }


        [TestMethod]
        public void SimpleWhereMethodTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "(([CustomerID] <= 50) AND ([CustomerCity] IS NULL))");
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

            Assert.AreEqual(sql, "(([DD] = '{00000000-0000-0000-0000-000000000000}') AND ([CustomerCity] IS NULL))");
        }


        [TestMethod]
        public void WhereMethodForParametersTest()
        {
            TestValueTypeParam(50);
        }

        private void TestValueTypeParam(int id)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.Key == id;

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);
            Assert.AreEqual(sql, "([Key] = 50)");
        }

        [TestMethod]
        public void WhereMethodForParametersTest2()
        {
            TestEntityParam(new TestValue { Id = 50 });
        }

        private void TestEntityParam(TestValue v)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => v.Id >= ct.CustomerID && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.AreEqual(sql, "((50 >= [CustomerID]) AND ([CustomerCity] IS NULL))");
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

            Assert.AreEqual(sql, "(([DD] = '{00000000-0000-0000-0000-000000000000}') AND ([CustomerCity] IS NULL))");
        }

        [TestMethod]
        public void SimpleWhereMethodTest3()
        {
            List<int> ids = new List<int>() { 40, 50 };//通过测试
            IEnumerable<int> ids2 = new List<int>() { 40, 50 };//通过测试
            int[] ids3 = new[] { 40, 50 };//通过测试

            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ids.Contains(ct.CustomerID);// && (SQLMethod.IsNull(ct.CustomerCity));
            Expression<Func<CustomersEntity, bool>> queryExp2 = ct => ids2.Contains(ct.CustomerID) || !ids3.Contains(ct.CustomerID);
            Expression<Func<CustomersEntity, bool>> queryExp3 = ct => ids.Contains(ct.CustomerID) && ids2.Contains(ct.Key) || !ids3.Contains(ct.CustomerID);
            Expression<Func<CustomersEntity, bool>> queryExp4 = ct => ct.TestBool == true && !ids.Contains(ct.CustomerID) && ct.CustomerCity.StartsWith("y");
            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);
            string sql2 = translate.Translate(queryExp2);
            string sql3 = translate.Translate(queryExp3);
            string sql4 = translate.Translate(queryExp4);

            Assert.AreEqual(sql, "([CustomerID] IN (40, 50))");
            Assert.AreEqual(sql2, "(([CustomerID] IN (40, 50)) OR (NOT ([CustomerID] IN (40, 50))))");
            Assert.AreEqual(sql3, "((([CustomerID] IN (40, 50)) AND ([Key] IN (40, 50))) OR (NOT ([CustomerID] IN (40, 50))))");
            Assert.AreEqual(sql4, "((([TestBool] = 1) AND (NOT ([CustomerID] IN (40, 50)))) AND ([CustomerCity] LIKE \'y%\'))");
        }

        [TestMethod]
        public void ColumnTest()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.Alias =="Abc";

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);
            Assert.AreEqual(sql, "([Alias__X] = 'Abc')");
        }
    }
}
