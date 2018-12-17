using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entitys.Test.Models;
using Keede.DAL.DDD.Repositories;

namespace Repository.Test
{
    public interface INews2Repository: IRepository<News2>
    {
        bool TestAdd(int id);
    }
}
