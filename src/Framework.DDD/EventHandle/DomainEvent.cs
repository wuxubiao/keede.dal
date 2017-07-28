using System;

namespace Framework.Core.DDD.EventHandle
{
    public class DomainEvent : IDomainEvent
    {
        public readonly DateTime OccurredOnTime;

        protected DomainEvent()
        {
            this.OccurredOnTime = DateTime.Now;
            this.IsRead = false;
        }

        public DateTime OccurredOn()
        {
            return this.OccurredOnTime;
        }

        public void Read()
        {
            this.IsRead = true;
        }

        public bool IsRead { get; private set; }
    }
}
