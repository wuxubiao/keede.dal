using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DDD.UnitTest
{
    public class NewsCustomRepository:SqlServerRepository<NewsCustom>, INewCustomRepository<NewsCustom>
    {
    }
}
