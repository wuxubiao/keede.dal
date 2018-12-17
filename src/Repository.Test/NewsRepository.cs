using System;
using System.Collections.Generic;
using Keede.DAL.DDD.Repositories;
using Entitys.Test.Models;


namespace Repository.Test
{
    public class News2Repository : SqlServerRepository<News2>, INews2Repository
    {
        public bool TestAdd(int id)
        {
            return false;
        }
    }
}
