using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.Common;
using Keede.DAL.DDD;
using Keede.DAL.RWSplitting;
using Keede.RepositoriesTests.Repositories;

namespace Keede.RepositoriesTests.Repositories
{

    public class BaseRepository<TEntity> :SqlServerRepository<TEntity> where TEntity : class, IEntity
    {
    }
}
