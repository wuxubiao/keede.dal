using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    /// <summary>
    /// 自动主键KEY，在Insert是会被过滤掉，适合用于自增主键和数据库自动赋值主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoKeyAttribute : Attribute
    {
    }
}
