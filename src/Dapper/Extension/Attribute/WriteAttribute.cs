using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    /// <summary>
    /// 标识写入，传递false无法Insert和Update
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WriteAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="write"></param>
        public WriteAttribute(bool write)
        {
            Write = write;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool Write { get; }
    }
}
