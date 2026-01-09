namespace MalignEngine;

[Serializable]
public struct ParentOf : IComponent
{
    [DataField("Parent", required: true)] 
    public Entity Parent;
}

public struct Children : IComponent
{
    public List<Entity> Values;
}

public class HierarchySystem : EntitySystem
{
    private readonly Dictionary<Entity, Entity> _knownParents = new();

    public IEnumerable<Entity> RootEntities
    {
        get
        {
            Query roots = new Query().Include<Transform>().Exclude<ParentOf>().Exclude<Destroyed>();
            List<Entity> root = new List<Entity>();
            World.Query(roots, root.Add);
            return root;
        }
    }

    public HierarchySystem(IServiceContainer serviceContainer) : base(serviceContainer) { }

    private void Attach(Entity child, Entity parent)
    {
        ref var children = ref World.GetOrAddComponent<Children>(parent);

        children.Values ??= new List<Entity>();
        children.Values.Add(child);
    }

    private void Detach(Entity child, Entity parent)
    {
        if (!World.HasComponent<Children>(parent))
        {
            return;
        }

        World.GetComponent<Children>(parent).Values.Remove(child);
    }

    public override void OnUpdate(float deltaTime)
    {
        World.Query(new Query().Include<ParentOf>(), entity =>
        {
            ref var parentOf = ref entity.Get<ParentOf>();

            if (!World.IsAlive(parentOf.Parent))
            {
                World.RemoveComponent<ParentOf>(entity);
                return;
            }

            if (!_knownParents.TryGetValue(entity, out var oldParent))
            {
                Attach(entity, parentOf.Parent);
                _knownParents[entity] = parentOf.Parent;
                return;
            }

            if (oldParent != parentOf.Parent)
            {
                Detach(entity, oldParent);
                Attach(entity, parentOf.Parent);
                _knownParents[entity] = parentOf.Parent;
            }
        });

        CleanupRemovedParents();

        World.Query(new Query().Include<Children>().Include<Destroyed>(), entity =>
        {
            ref var children = ref entity.Get<Children>();

            foreach (var child in children.Values)
            {
                if (!child.Has<Destroyed>())
                {
                    EntityManager.Destroy(child);
                }
            }
        });

        World.Query(new Query().Include<ParentOf>().Include<Destroyed>(), entity =>
        {
            var parent = entity.Get<ParentOf>().Parent;

            if (parent.IsAlive() && parent.Has<Children>())
            {
                parent.Get<Children>().Values.Remove(entity);
            }

            entity.Remove<ParentOf>();
        });

        World.Query(new Query().Include<ParentOf>().Exclude<Destroyed>(), entity =>
        {
            var parent = entity.Get<ParentOf>().Parent;

            if (!parent.IsAlive() || parent.Has<Destroyed>())
            {
                entity.Remove<ParentOf>();
            }
        });

        World.Query(new Query().Include<Children>(), entity =>
        {
            ref var children = ref entity.Get<Children>();

            children.Values.RemoveAll(
                c => !c.IsAlive() || c.Has<Destroyed>()
            );

            if (children.Values.Count == 0)
            {
                entity.Remove<Children>();
            }
        });


    }

    private void CleanupRemovedParents()
    {
        var toRemove = new List<Entity>();

        foreach (var (child, parent) in _knownParents)
        {
            if (!World.HasComponent<ParentOf>(child))
            {
                Detach(child, parent);
                toRemove.Add(child);
            }
        }

        foreach (var e in toRemove)
        {
            _knownParents.Remove(e);
        }
    }
}