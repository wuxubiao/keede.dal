using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Tests
{
    [TestClass()]
    public class SqlMapperExtensionsTests
    {
        [TestMethod()]
        public void GetSelectMatchTest()
        {
            SqlMapperExtensions.GetSelectColumnReplaceToCount("select id,dd from news (select dff from sdfdf)");
        }
    }
}