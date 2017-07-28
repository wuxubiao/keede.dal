namespace Keede.DAL.DDD
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAggregateRoot : IAggregateRoot<int>, IEntity
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public interface IAggregateRoot<TPrimaryKey> : IEntity<TPrimaryKey>
    {

    }
}