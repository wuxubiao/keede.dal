using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;

namespace Keede.DAL.DomainBase.UnitTest.Models
{
    [Table("PersonDetail")]
    public class PersonDetail
    {
        [ExplicitKey]
        public Guid PersonId { get; set; }

        public string City { get; set; }
    }
}
