using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Tests
{
    public partial class CustomersEntity
    {
        ///<summary>
        ///
        ///</summary>               
        public Int32 CustomerID { set; get; }

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

        ///<summary>
        ///
        ///</summary>               
        public Nullable<Boolean> IsActive { set; get; }

    }
}
