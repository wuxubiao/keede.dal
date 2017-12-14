using Dapper.Extension;
using Keede.DAL.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DDD.UnitTest.Models
{
    [Table("NewsMultiId"), TypeMapper]
    public class NeNewsMultiIdws : IEntity
    {
        [ExplicitKey]
        public int Id { get; set; }

        [ExplicitKey]
        public int GId { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

        [IgnoreRead, IgnoreWrite]
        public string ContentTest { get; set; }
    }
}