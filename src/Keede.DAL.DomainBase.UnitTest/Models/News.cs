using Dapper.Extension;
using Keede.DAL.DomainBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DomainBase.UnitTest.Models
{
    [Table("News"), TypeMapper]
    public class News : IEntity
    {
        //[ExplicitKey, Column("GId")]
        [ExplicitKey]
        public int GId { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

        [IgnoreRead, Computed]
        public string ContentTest { get; set; }
    }
}