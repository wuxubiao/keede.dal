using Dapper.Extension;
using Framework.Core.DomainBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.UnitTest.Models
{
    [Table("News")]
    public class News : Entity
    {
        [ExplicitKey]
        public int id { get; set; }

        public string title { get; set; }
    }
}
