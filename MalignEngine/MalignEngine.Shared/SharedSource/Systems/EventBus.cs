using Arch.Core;
using Arch.Core.Extensions;
using nkast.Aether.Physics2D.Dynamics;
using System.ComponentModel.Design;
using System.Reflection;
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

    public class EventSystem : BaseSystem
    {
        [Dependency]
        protected WorldSystem WorldSystem = default!;

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

        public void RegisterComponent<T>()
        {
            WorldSystem.World.SubscribeComponentAdded((in Entity entity, ref T component) =>
            {
                RaiseLocalEvent<ComponentAddedEvent, T>(new ComponentAddedEvent(entity.Reference()));
            });

            WorldSystem.World.SubscribeComponentRemoved((in Entity entity, ref T component) =>
            {
                RaiseLocalEvent<ComponentRemovedEvent, T>(new ComponentRemovedEvent(entity.Reference()));
            });
        }
        public override void Initialize()
        {
            WorldSystem.World.SubscribeEntityCreated(OnEntityCreated);
            WorldSystem.World.SubscribeEntityDestroyed(OnEntityDestroyed);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in loadedAssemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsGenericType) { continue; }

                    RegisterComponent attribute = type.GetCustomAttribute<RegisterComponent>();
                    if (attribute != null)
                    {
                        var mi = typeof(EventSystem).GetMethod(nameof(RegisterComponent));
                        var fooRef = mi.MakeGenericMethod(type);
                        fooRef.Invoke(this, null);
                    }
                }
            }
        }

        private void OnEntityCreated(in Entity entity)
        {
            RaiseLocalEvent(new EntityCreatedEvent(entity.Reference()));
        }

        private void OnEntityDestroyed(in Entity entity)
        {
            RaiseLocalEvent(new EntityDestroyedEvent(entity.Reference()));
        }

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