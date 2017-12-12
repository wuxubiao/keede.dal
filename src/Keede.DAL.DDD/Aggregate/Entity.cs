using System;
using Dapper.Extension;

namespace Keede.DAL.DDD
{
    /// <summary>
    /// A shortcut of <see cref="Entity{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
    /// </summary>
    [Serializable]
    public abstract class Entity : IEntity
    {

    }
}
