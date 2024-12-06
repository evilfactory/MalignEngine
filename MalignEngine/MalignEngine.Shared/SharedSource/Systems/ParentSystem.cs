using Arch.Core;
using Arch.Core.Extensions;

namespace MalignEngine
{
    public struct ParentOf : IComponent
    {
        public EntityReference Parent;
    }

    public struct Children : IComponent
    {
        public HashSet<EntityReference> Childs;
    }

    public class ParentSystem : EntitySystem
    {
        public override void OnInitialize()
        {
            EntityEventSystem.SubscribeLocalEvent<ComponentAddedEvent, ParentOf>(ParentAddedComponent);
            EntityEventSystem.SubscribeLocalEvent<ComponentRemovedEvent, ParentOf>(ParentRemovedComponent);
        }

        public IEnumerable<Entity> RootEntities
        {
            get
            {
                var query = new QueryDescription().WithNone<ParentOf>();
                List<Entity> entities = new List<Entity>();
                World.Query(query, (Entity entity) =>
                {
                    entities.Add(entity);
                });
                return entities.ToArray();
            }
        }

        private void ParentRemovedComponent(ComponentRemovedEvent removeEvent)
        {
            Entity entity = removeEvent.Entity;

            if (entity.Has<Children>())
            {
                ref Children children = ref entity.Get<Children>();

                foreach (EntityReference child in children.Childs)
                {
                    if (child.Entity.Has<ParentOf>())
                    {
                        child.Entity.Remove<ParentOf>();
                    }
                }
            }

            if (entity.Has<ParentOf>())
            {
                ref ParentOf parentOf = ref entity.Get<ParentOf>();

                if (parentOf.Parent.Entity.Has<Children>())
                {
                    ref Children children = ref parentOf.Parent.Entity.Get<Children>();

                    children.Childs.Remove(entity.Reference());
                }
            }
        }

        private void ParentAddedComponent(ComponentAddedEvent addedEvent)
        {
            Entity entity = addedEvent.Entity;

            ParentOf parentOf = entity.Get<ParentOf>();

            ref Children children = ref parentOf.Parent.Entity.AddOrGet<Children>();

            if (children.Childs == null) { children.Childs = new HashSet<EntityReference>(); }

            children.Childs.Add(entity.Reference());
        }
    }
}