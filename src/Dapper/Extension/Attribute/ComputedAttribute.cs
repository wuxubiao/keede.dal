using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    /// <summary>
    /// 标识已计算过，此特性在Insert和Update都会被过滤掉
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComputedAttribute : Attribute
    {
    }
}
