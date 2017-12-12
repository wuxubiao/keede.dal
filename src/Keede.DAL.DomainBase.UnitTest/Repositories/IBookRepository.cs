using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keede.DAL.DDD.Repositories;
using Keede.RepositoriesTests.ValueObject;

namespace Keede.RepositoriesTests.Repositories
{
    public interface IBookRepository : IRepository<Book>
    {
    }
}
