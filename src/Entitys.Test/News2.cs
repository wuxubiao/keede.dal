using Dapper.Extension;
using Keede.DAL.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entitys.Test.Models
{
    [Table("News2")]
    public class News2 : IEntity
    {
//        [ExplicitKey]
        [Key]
        public int Id { get; set; }

        //[ExplicitKey]
//        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

//        public Guid Test1 { get; set; }
//        public int Num { get; set; }

        public DateTime? CreateDateTime { get; set; }

        [IgnoreRead, IgnoreWrite]
        public string ContentTest { get; set; }
    }
}