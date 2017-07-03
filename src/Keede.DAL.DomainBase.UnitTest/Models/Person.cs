using System;
using Dapper.Extension;
using Keede.DAL.DomainBase;

namespace Keede.DAL.DomainBase.UnitTest.Models
{
    [Table("Person")]
    public class Person:Entity<Guid>
    {
        [ExplicitKey]
        public override Guid Id { get; set; }

        public string Name { get; set; }

        public byte Age { get; set; }
    }
}
