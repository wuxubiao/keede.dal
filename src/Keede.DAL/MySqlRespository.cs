using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keede.DAL.Interface;
using PagedList;

namespace Keede.DAL
{
    public class MySqlRespository : IRepository<IEntity>
    {
        public bool Add(IEntity data)
        {
            throw new NotImplementedException();
        }

        public bool BulkInsert(IEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public bool Save(IEntity data)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IEntity condition)
        {
            throw new NotImplementedException();
        }

        public IEntity Get(string sql, object parameterObject = null)
        {
            throw new NotImplementedException();
        }

        public IEntity GetById(dynamic id)
        {
            throw new NotImplementedException();
        }

        public IEntity GetById(dynamic id, bool isUpdateLock)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> GetList(string sql, object parameterObject = null)
        {
            throw new NotImplementedException();
        }

        public IList<IEntity> GetAll()
        {
            throw new NotImplementedException();
        }

        public PagedList<IEntity> GetPagedList(string whereSql, string orderBy, object parameterObjects, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}