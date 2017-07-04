using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extension
{
    /// <summary>
    /// 明确主键，可用于Insert时添加进去
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExplicitKeyAttribute : Attribute
    {
    }
}
