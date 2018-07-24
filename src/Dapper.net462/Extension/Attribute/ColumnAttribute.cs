using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
