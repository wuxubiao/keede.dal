using System;

namespace Framework.DDD.EventHandle
{
    public interface IDomainEventSubscriber
    {
        Type SubscribedToEventType();

        void Handle(object domainEvent);
    }
}
