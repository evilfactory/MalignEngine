using Arch.Core;
using Arch.Core.Extensions;
using System.ComponentModel.Design;
using System.Security.Cryptography;

namespace MalignEngine
{

    public abstract class EventArgs { }
    public abstract class EntityEventArgs : EventArgs
    {
        public Entity Entity { get; private set; }

        public EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }
    }

    public class EventBus
    {
        private class EventSubscription
        {
            public Delegate Handler { get; private set; }
            public Type? ComponentType;

            public EventSubscription(Delegate handler, Type? componentType = null)
            {
                Handler = handler;
                ComponentType = componentType;
            }
        }

        private List<EventSubscription> eventSubscriptions = new List<EventSubscription>();

        public void SubscribeLocalEvent<T>(Action<T> handler) where T : EventArgs
        {
            eventSubscriptions.Add(new EventSubscription(handler));
        }

        public void SubscribeLocalEvent<TEvent, TComp>(Action<TEvent> handler)
        {
            eventSubscriptions.Add(new EventSubscription(handler, typeof(TComp)));
        }

        public void RaiseLocalEvent<TEvent>(TEvent args) where TEvent : EventArgs
        {
            foreach (var subscription in eventSubscriptions)
            {
                if (subscription.Handler is Action<TEvent> handler)
                {
                    handler(args);
                }
            }
        }

        public void RaiseLocalEvent<TEvent, TComp>(TEvent args) where TEvent : EntityEventArgs
        {
            foreach (var subscription in eventSubscriptions)
            {
                if (subscription.Handler is Action<TEvent> handler && subscription.ComponentType == typeof(TComp))
                {
                    handler(args);
                }
            }
        }
    }
}