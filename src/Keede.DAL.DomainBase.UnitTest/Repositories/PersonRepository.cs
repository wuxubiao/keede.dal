using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Dapper.Extension;
using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest.Models;
using Keede.RepositoriesTests;

namespace Keede.DAL.DDD.UnitTest
{
    public class PersonRepository:SqlServerRepository<Person>
    {
        public void Test(List<Guid> userIds)
        {
            string tableName = "##UserIds";
            var idViewList = new IdView().GetIdViewList(userIds.Distinct().ToList());

            string sqlStr = string.Format(@"CREATE TABLE {0}(ID UNIQUEIDENTIFIER)", tableName);
            var conn = OpenDbConnection(false);
            var num = conn.Execute(sqlStr);

            var result = BatchAdd(idViewList, tableName);
        }
    }

    public class IdView
    {
        [ExplicitKey]
        public Guid ID { get; set; }

        internal List<IdView> GetIdViewList(IList<Guid> ids)
        {
            if (ids == null || !ids.Any()) return null;
            return ids.Select(id => new IdView { ID = id }).ToList();
        }
    }
}
