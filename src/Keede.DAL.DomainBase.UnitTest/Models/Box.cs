using Dapper.Extension;
using System;
using Keede.DAL.DDD;

namespace Keede.DAL.DDD.UnitTest.Models
{
    /// <summary>
    /// 盒子
    /// </summary>
    [Table("Box")]
    public class Box : IEntity
    {
        [Key]
        public Guid BoxId { get;  set; }

        public string BoxName { get;  set; }

        public int LengthCM { get;  set; }

        public int WidthCM { get;  set; }

        public int HeightCM { get;  set; }

        public bool IsUse { get;  set; }

        public DateTime LastModifyTime { get; private set; }
    }
}
