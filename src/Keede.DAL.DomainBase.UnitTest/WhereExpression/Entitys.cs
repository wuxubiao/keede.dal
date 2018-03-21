using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Tests
{
    public class CustomersEntity
    {
        ///<summary>
        ///
        ///</summary>               
        public Int32 CustomerID { set; get; }

        public Guid DD { set; get; }
        ///<summary>
        ///
        ///</summary>               
        public String CustomerNumber { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public String CustomerName { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public String CustomerCity { set; get; }

        public bool TestBool { set; get; }
        ///<summary>
        ///
        ///</summary>               
        public Nullable<Boolean> IsActive { set; get; }

    }
}
