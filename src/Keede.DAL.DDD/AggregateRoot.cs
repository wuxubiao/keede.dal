namespace Keede.DAL.DDD
{
    /// <summary>
    /// 
    /// </summary>
    public class AggregateRoot : AggregateRoot<int>, IAggregateRoot
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    {
    }
}