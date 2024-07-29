using Arch.Core;
using Arch.Core.Extensions;
using System.ComponentModel.Design;
using System.Reflection;

namespace MalignEngine
{
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


    public class EntityEventSystem : EntitySystem
    {

        public void RegisterComponent<T>()
        {

            World.SubscribeComponentAdded((in Entity entity, ref T component) =>
            {
                EventBus.RaiseLocalEvent<ComponentAddedEvent, T>(new ComponentAddedEvent(entity.Reference()));
            });

            World.SubscribeComponentRemoved((in Entity entity, ref T component) =>
            {
                EventBus.RaiseLocalEvent<ComponentRemovedEvent, T>(new ComponentRemovedEvent(entity.Reference()));
            });
        }

        public override void Initialize()
        {
            World.SubscribeEntityCreated(OnEntityCreated);
            World.SubscribeEntityDestroyed(OnEntityDestroyed);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in loadedAssemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsGenericType) { continue; }

                    RegisterComponent attribute = type.GetCustomAttribute<RegisterComponent>();
                    if (attribute != null)
                    {
                        var mi = typeof(EntityEventSystem).GetMethod(nameof(RegisterComponent));
                        var fooRef = mi.MakeGenericMethod(type);
                        fooRef.Invoke(this, null);
                    }
                }
            }
        }

        private void OnEntityCreated(in Entity entity)
        {
            EventBus.RaiseLocalEvent(new EntityCreatedEvent(entity.Reference()));
        }

        private void OnEntityDestroyed(in Entity entity)
        {
            EventBus.RaiseLocalEvent(new EntityDestroyedEvent(entity.Reference()));
        }
    }

}