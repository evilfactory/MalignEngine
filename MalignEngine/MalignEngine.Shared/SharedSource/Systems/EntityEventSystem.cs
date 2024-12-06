using Arch.Core;
using Arch.Core.Extensions;
using nkast.Aether.Physics2D.Dynamics;
using System.ComponentModel.Design;
using System.Reflection;
using System.Security.Cryptography;

namespace MalignEngine
{
    public abstract class EntityEventArgs
    {
        public Entity Entity { get; private set; }

        public EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }
    }
    public class EntityCreatedEvent : EntityEventArgs
    {
        public EntityCreatedEvent(EntityReference entity) : base(entity)
        {
        }
    }
    public class EntityDestroyedEvent : EntityEventArgs
    {
        public EntityDestroyedEvent(EntityReference entity) : base(entity)
        {
        }
    }

    public class ComponentAddedEvent : EntityEventArgs
    {
        public ComponentAddedEvent(EntityReference entity) : base(entity)
        {
        }
    }

    public class ComponentRemovedEvent : EntityEventArgs
    {
        public ComponentRemovedEvent(EntityReference entity) : base(entity)
        {
        }
    }

    public class EntityEventSystem : BaseSystem
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
        public override void OnInitialize()
        {
            WorldSystem.World.SubscribeEntityCreated(OnEntityCreated);
            WorldSystem.World.SubscribeEntityDestroyed(OnEntityDestroyed);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Type type in loadedAssemblies.SelectMany(assembly => assembly.GetTypes()))
            {
                if (type.IsGenericType) { continue; }

                if (type.IsAssignableTo(typeof(IComponent)))
                {
                    MethodInfo method = typeof(EntityEventSystem).GetMethod("RegisterComponent");
                    MethodInfo generic = method.MakeGenericMethod(type);
                    generic.Invoke(this, null);
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

        public void RaiseLocalEvent<TEvent>(TEvent args) where TEvent : EntityEventArgs
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