using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Framework.Core.DDD.EventHandle
{
    public class DomainEventConsistentQueue : IDisposable
    {
        private static readonly ThreadLocal<List<IDomainEvent>> _domainEvents = new ThreadLocal<List<IDomainEvent>>();
        private static readonly ThreadLocal<bool> _publishing = new ThreadLocal<bool> { Value = false };

        private DomainEventConsistentQueue() { }
        private static DomainEventConsistentQueue _current;
        /// <summary>
        /// 获取当前的领域事件一致性队列。
        /// 由于使用了线程本地存储变量，此处为单例模式。
        /// </summary>
        /// <returns></returns>
        public static DomainEventConsistentQueue Current()
        {
            if (_current != null)
                return _current;
            var temp = new DomainEventConsistentQueue();
            Interlocked.CompareExchange(ref _current, temp, null);
            return _current;
        }

        public bool IsEmpty()
        {
            return _domainEvents.Value == null;
        }

        public void RegisterEvent(IDomainEvent domainEvent)
        {
            if (_publishing.Value)
            {
                throw new ApplicationException("当前事件一致性队列已被发布，无法添加新的事件！");
            }

            var domainEvents = _domainEvents.Value;
            if (domainEvents == null)
            {
                domainEvents = new List<IDomainEvent>();
                _domainEvents.Value = domainEvents;
            }

            if (domainEvents.Any(ent => ent == domainEvent))  //防止相同事件被重复添加
                return;

            //TODO: 相同领域对象的领域事件可以考虑做合并提交。

            domainEvents.Add(domainEvent);
        }

        public void Clear()
        {
            _domainEvents.Value = null;
            _publishing.Value = false;
        }

        public void PublishEvents()
        {
            if (_publishing.Value)
            {
                return;
            }

            if (_domainEvents.Value == null)
                return;

            try
            {
                _publishing.Value = true;
                foreach (var domainEvent in _domainEvents.Value)
                {
                    DomainEventBus.Instance().Publish(domainEvent);
                }
            }
            finally
            {
                Clear();
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
