using Keede.DAL.DomainBase;
using Keede.DAL.DomainBase.Repositories;
using System;

namespace Keede.RepositoriesTests.Repositories
{
    public abstract class DapperRepositoryBase<T> : DapperRepositoryBase<T, Guid> where T : Entity<Guid> { }

    public abstract class DapperRepositoryBase<T, TKey> : SqlServerRepository<T> where T : Entity<TKey> { }
}
