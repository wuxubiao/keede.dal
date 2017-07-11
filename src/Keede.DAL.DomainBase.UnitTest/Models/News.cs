using Dapper.Extension;
using Keede.DAL.DomainBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DomainBase.UnitTest.Models
{
    [Table("News")]
    public class News : Entity<int>
    {
        //[ExplicitKey]
        //public int GId { get; set; }

        //[AutoKey("ddd")]
        //public int GId { get; set; }

        //[ExplicitKey]
        //public override int Id { get; set; }

        [ExplicitKey]
        public override int Id { get; set; }

        //[ExplicitKey]
        public string Title { get; set; }

        public string Content { get; set; }
    }
}