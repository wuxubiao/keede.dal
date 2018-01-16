using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;
using Keede.DAL.DDD;

namespace Keede.RepositoriesTests.Models
{
    [Table("CouponNo")]
    public class CouponNoEntity : IEntity
    {
        [ExplicitKey]
        public int No { get; set; }

        public Guid PromotionID { get; set; }

        public string CouponNo { get; set; }

        public DateTime UseStartTime { get; set; }

        public DateTime UseEndTime { get; set; }

        public Guid MemberID { get; set; }

        public DateTime BuildTime { get; set; }

        public bool IsUsed { get; set; }
    }
}
