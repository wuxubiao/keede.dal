using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;

namespace Keede.DAL.DomainBaseTests.Models
{
    [Table("PersonScore")]
    public class PersonScore
    {
        [ExplicitKey]
        public Guid PersonId { get; set; }

        public string Name { get; set; }

        public byte Age { get; set; }
    }
}
