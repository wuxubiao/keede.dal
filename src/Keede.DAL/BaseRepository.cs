using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keede.DAL.Interface;
using PagedList;

namespace Keede.DAL
{
    public abstract class BaseRepository : IRepository<IEntity>
    {
        public virtual bool Add(IEntity data)
        {
            throw new NotImplementedException();
        }

        public virtual bool BulkInsert(IEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public virtual bool Save(IEntity data)
        {
            throw new NotImplementedException();
        }

        public virtual bool Remove(IEntity condition)
        {
            throw new NotImplementedException();
        }

        public virtual IEntity Get(string sql, object parameterObject = null)
        {
            throw new NotImplementedException();
        }

        public virtual IEntity GetById(dynamic id)
        {
            throw new NotImplementedException();
        }

        public virtual IEntity GetById(dynamic id, bool isUpdateLock)
        {
            throw new NotImplementedException();
        }

        public virtual IList<IEntity> GetList(string sql, object parameterObject = null)
        {
            throw new NotImplementedException();
        }

        public virtual IList<IEntity> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual PagedList<IEntity> GetPagedList(string whereSql, string orderBy, object parameterObjects, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}