using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.Common;
using Keede.DAL.RWSplitting;
using Keede.RepositoriesTests.Repositories;

namespace Keede.DAL.DDD.UnitTest
{
    public class NewsRepository : SqlServerRepository<News>, INewsRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        //public NewsRepository(bool isReadDb = true)
        //{
        //    base.SetDbConnection(isReadDb);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="isReadDb"></param>
        /// <returns></returns>
        //public NewsRepository(string dbName, bool isReadDb = true)
        //{
        //    base.SetDbConnection(dbName, isReadDb);
        //}

        public bool TestAdd(int id)
        {
            return false;
        }
    }
}
