using Dapper.Extension;
using Keede.DAL.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DDD.UnitTest.Models
{
    [Table("NewsConstructor"), TypeMapper]
    public class NewsConstructor : IEntity
    {
        //[ExplicitKey, Column("GId")]
        [ExplicitKey]
        public int GId { get; set; }

        [ExplicitKey]
        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

        [IgnoreRead, Computed]
        public string ContentTest { get; set; }

        private NewsConstructor()
        {
        }
    }
}