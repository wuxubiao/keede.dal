using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreReadAttribute : Attribute
    {
    }
}
