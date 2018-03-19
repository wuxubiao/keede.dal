using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RWSplitDbNameAttribute : System.Attribute
    {
        /// <summary>
        /// 
        /// 构造函数
        /// </summary>
        /// <param name="dbName"></param>
        public RWSplitDbNameAttribute(string dbName)
        {
            Name = dbName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }
    }
}
