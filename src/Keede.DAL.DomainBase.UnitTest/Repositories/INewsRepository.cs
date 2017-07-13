using Keede.DAL.DomainBase.Repositories;
using Keede.DAL.DomainBase.UnitTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DomainBase.UnitTest
{
    public interface INewsRepository: IRepository<News>
    {
        bool TestAdd(int id);
    }
}
