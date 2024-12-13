using Arch.Core;
using Arch.Core.Extensions;
using nkast.Aether.Physics2D.Dynamics;
using Silk.NET.SDL;
using System.ComponentModel.Design;
using System.Reflection;
using System.Security.Cryptography;

namespace MalignEngine
{
    public abstract class EntityEventArgs { }

    public class EntityEventSystem : IService
    {
        [Dependency]
        private EntityManagerService EntityManager = default!;

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

        public void SubscribeEvent<T>(Action<T> handler) where T : EntityEventArgs
        {
            eventSubscriptions.Add(new EventSubscription(handler));
        }

        public void SubscribeEvent<TEvent, TComp>(Action<EntityRef, TEvent> handler)
        {
            eventSubscriptions.Add(new EventSubscription(handler, typeof(TComp)));
        }

        // Broadcasts an event to all subscribers of the event
        public void RaiseEvent<T>(T args) where T : EntityEventArgs
        {
            foreach (var subscription in eventSubscriptions)
            {
                if (subscription.Handler is Action<T> handler)
                {
                    handler(args);
                }
            }
        }

        // Broadcasts an event to all subscribers of the event that are interested in the component types of this entity
        public void RaiseEvent<T>(EntityRef entity, T args)
        {
            HashSet<Type> componentTypes = EntityManager.World.GetComponents(entity).Select(comp => comp.GetType()).ToHashSet();

            foreach (var subscription in eventSubscriptions)
            {
                if (subscription.Handler is Action<EntityRef, T> handler && componentTypes.Contains(subscription.ComponentType))
                {
                    handler(entity, args);
                }
            }
        }

        public void RaiseEvent<TEvent, TComp>(EntityRef entity, TEvent args) where TEvent : EntityEventArgs
        {
            foreach (var subscription in eventSubscriptions)
            {
                if (subscription.Handler is Action<EntityRef, TEvent> handler && subscription.ComponentType == typeof(TComp))
                {
                    handler(entity, args);
                }
            }
        }
    }
}