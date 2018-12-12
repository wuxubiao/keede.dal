using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extension;

namespace Dapper.Extensions.Tests
{
    public class CustomersEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public int Key;

        ///<summary>
        ///
        ///</summary>               
        public Int32 CustomerID { set; get; }

        public Guid DD { set; get; }
        ///<summary>
        ///
        ///</summary>               
        public string CustomerNumber { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public string CustomerName { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public string CustomerCity { set; get; }

        public bool TestBool;

        public DateTime CreateDateTime;

        [Column("Alias__X")]
        public string Alias { get; set; }

        public int? NullableInt { get; set; }

        public DateTime? NullableDate { get; set; }

        ///<summary>
        ///
        ///</summary>               
        public Nullable<Boolean> IsActive { set; get; }


    }
}
