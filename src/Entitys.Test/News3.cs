using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;
using Keede.DAL.DDD;

namespace Entitys.Test
{
    [Table("News3")]
    public class News3 : IEntity
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        //[ExplicitKey]
        //        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

        public int? OrderNum { get; set; }

        //        public Guid Test1 { get; set; }
        //        public int Num { get; set; }

        public DateTime CreateDateTime { get; set; }

        [IgnoreRead, IgnoreWrite]
        public string ContentTest { get; set; }
    }
}
