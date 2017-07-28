using System;

namespace Framework.Core.DDD.EventHandle
{
    public interface IDomainEventSubscriber
    {
        Type SubscribedToEventType();

        void Handle(object domainEvent);
    }
}
