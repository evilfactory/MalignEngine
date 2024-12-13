namespace MalignEngine
{
    public struct ParentOf : IComponent
    {
        public EntityRef Parent;
    }

    public struct Children : IComponent
    {
        public HashSet<EntityRef> Childs;
    }

    public class ParentSystem : EntitySystem
    {
        public override void OnInitialize()
        {
            EntityEventSystem.SubscribeEvent<ComponentAddedEvent, ParentOf>(ParentAddedComponent);
            EntityEventSystem.SubscribeEvent<ComponentRemovedEvent, ParentOf>(ParentRemovedComponent);
            EntityEventSystem.SubscribeEvent<EntityDestroyedEvent>(EntityDestroyed);
        }

        public IEnumerable<EntityRef> RootEntities
        {
            get
            {
                var query = EntityManager.World.CreateQuery().WithNone<ParentOf>();
                List<EntityRef> entities = new List<EntityRef>();
                EntityManager.World.Query(query, (EntityRef entity) =>
                {
                    entities.Add(entity);
                });
                return entities.ToArray();
            }
        }
        private void EntityDestroyed(EntityDestroyedEvent entity)
        {
            if (entity.Entity.Has<Children>())
            {
                // Destroy all children as well
                ref Children children = ref entity.Entity.Get<Children>();

                foreach (EntityRef child in children.Childs)
                {
                    child.Destroy();
                }
            }
        }

        private void ParentRemovedComponent(EntityRef entity, ComponentRemovedEvent removeEvent)
        {
            if (entity.Has<Children>())
            {
                ref Children children = ref entity.Get<Children>();

                foreach (EntityRef child in children.Childs)
                {
                    if (child.Has<ParentOf>())
                    {
                        child.Remove<ParentOf>();
                    }
                }
            }

            if (entity.Has<ParentOf>())
            {
                ref ParentOf parentOf = ref entity.Get<ParentOf>();

                if (parentOf.Parent.Has<Children>())
                {
                    ref Children children = ref parentOf.Parent.Get<Children>();

                    children.Childs.Remove(entity);
                }
            }
        }

        private void ParentAddedComponent(EntityRef entity, ComponentAddedEvent addedEvent)
        {
            ParentOf parentOf = entity.Get<ParentOf>();

            ref Children children = ref parentOf.Parent.AddOrGet<Children>();

            if (children.Childs == null) { children.Childs = new HashSet<EntityRef>(); }

            children.Childs.Add(entity);
        }
    }
}