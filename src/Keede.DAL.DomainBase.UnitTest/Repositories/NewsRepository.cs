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

        public void TestTestBatchAdd()
        {
            using (var conn = OpenDbConnection(false, true))
            {
                conn.Execute("delete news ", transaction: DbTransaction);

                IList<News> list = new List<News>();
                var news1 = new News();
                news1.Id = 9998;
                news1.Title = "title1" + DateTime.Now;
//                news1.Test1 = new Guid();
                list.Add(news1);

                var news2 = new News();
                news2.Id = 9999;
                news2.Title = "title2" + DateTime.Now;
                list.Add(news2);
                var result = BatchAdd(list);

                DbTransaction.Commit();
            }
        }
    }
}
