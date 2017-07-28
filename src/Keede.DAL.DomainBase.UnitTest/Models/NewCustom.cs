﻿using Dapper.Extension;
using Keede.DAL.DDD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DDD.UnitTest.Models
{
    [Table("NewsCustom")]
    public class NewsCustom : Entity<int>
    {
        [Key]
        public override int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        [IgnoreRead, Computed]
        public string ContentTest { get; set; }
    }
}