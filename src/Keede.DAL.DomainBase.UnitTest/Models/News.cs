using Dapper.Extension;
using Keede.DAL.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DDD.UnitTest.Models
{
    [Table("News"), TypeMapper]
    public class News : IEntity
    {
        //[ExplicitKey, Column("GId")]
        [ExplicitKey]
        public int GId { get; set; }

        //[ExplicitKey]
        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

        public Guid Test1 { get; set; }

        [IgnoreRead, IgnoreWrite]
        public string ContentTest { get; set; }
    }
}