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


    public sealed class WorldSystem : BaseSystem
    {
        public World World
        {
            get => world;
        }

        private World world = default!;

        public WorldSystem()
        {
            world = World.Create();
            IoCManager.Register(world);
        }

    }

}