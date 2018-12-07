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
        public void LikeTest()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerName.Contains("X") && ct.CustomerCity.Contains("Y") ||ct.CustomerName.StartsWith("A");
            Expression<Func<CustomersEntity, bool>> queryExp2 = ct => ct.CustomerName.StartsWith("X");
            Expression<Func<CustomersEntity, bool>> queryExp3 = ct => ct.CustomerName.EndsWith("X");

            var builder = new WhereBuilder();
            var sql = builder.ToSql(queryExp1);
        }
    }
}
