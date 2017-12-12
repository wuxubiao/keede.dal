using System;

namespace Keede.DAL.DDD
{
    /// <summary>
    /// 
    /// </summary>
    public class AggregateRoot : AggregateRoot<int>
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPrimaryKey"></typeparam>
    [Serializable]
    public abstract class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    {
    }
}