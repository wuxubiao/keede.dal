using Dapper.Extension;

namespace Keede.DAL.DDD.UnitTest
{
    [Table("News"),TypeMapper]
    public class News : IEntity
    {
//        [ExplicitKey]
        [Key]
        public int GId { get; set; }

        //[ExplicitKey]
        [Column("Title")]
        public string Title { get; set; }

        //[Column("Content")]
        public string Content { get; set; }

        [IgnoreRead, IgnoreWrite]
        public string ContentTest { get; set; }
    }
}