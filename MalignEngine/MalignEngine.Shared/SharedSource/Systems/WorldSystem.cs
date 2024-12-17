using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;

namespace MalignEngine;

public enum EntityLifeStage
{
    Created,
    Terminating,
    Deleted,
}

public enum ComponentLifeStage
{
    Adding,
    Added,
    Initializing,
    Initialized,
    Starting,
    Running,
    Stopping,
    Stopped,
    Removing,
    Deleted,
}

public class EntityMetaData : IComponent
{
    public EntityLifeStage LifeStage;
    public Dictionary<Type, ComponentLifeStage> ComponentLifeStages;
}

public class EntityCreatedEvent : EntityEventArgs
{
    public EntityRef Entity;

    public EntityCreatedEvent(EntityRef entity)
    {
        Entity = entity;
    }
}
public class EntityDestroyedEvent : EntityEventArgs
{
    public EntityRef Entity;

    public EntityDestroyedEvent(EntityRef entity)
    {
        Entity = entity;
    }
}

public class ComponentAddedEvent : EntityEventArgs { }
public class ComponentInitEvent : EntityEventArgs { }
public class ComponentStartEvent : EntityEventArgs { }
public class ComponentStopEvent : EntityEventArgs { }
public class ComponentRemovedEvent : EntityEventArgs { }
public class ComponentDeletedEvent : EntityEventArgs { }

public delegate void ForEachWithEntity(EntityRef entity);
public delegate void ForEachWithEntity<T0>(EntityRef entity, ref T0 t0Component);
public delegate void ForEachWithEntity<T0, T1>(EntityRef entity, ref T0 t0Component, ref T1 t1Component);
public delegate void ForEachWithEntity<T0, T1, T2>(EntityRef entity, ref T0 t0Component, ref T1 t1Component, ref T2 t2Component);
public delegate void ForEachWithEntity<T0, T1, T2, T3>(EntityRef entity, ref T0 t0Component, ref T1 t1Component, ref T2 t2Component, ref T3 t3Component);
public delegate void ForEachWithEntity<T0, T1, T2, T3, T4>(EntityRef entity, ref T0 t0Component, ref T1 t1Component, ref T2 t2Component, ref T3 t3Component, ref T4 t4Component);

public sealed class EntityManagerService : IService, IInit, IPostUpdate
{
    [Dependency]
    private EntityEventSystem EntityEventSystem = default!;

    [Dependency]
    private ILoggerService LoggerService = default!;

    private ILogger Logger;

    public WorldRef World
    {
        get => world;
    }

    private WorldRef world = default!;

    public void OnInitialize()
    {
        Logger = LoggerService.GetSawmill("EntityManager");

        world = new WorldRef(Logger, EntityEventSystem);
        IoCManager.Register(world);
    }

    public void OnPostUpdate(float deltaTime)
    {
        World.Update();
    }
}

public class WorldRef
{
    internal World world;

    private ILogger logger;
    private EntityEventSystem entityEventSystem;

    public WorldRef(ILogger logger, EntityEventSystem eventSystem)
    {
        this.logger = logger;
        world = World.Create();
        entityEventSystem = eventSystem;
    }

    public EntityRef CreateEntity()
    {
        EntityRef entity = new EntityRef(this, world.Create());
        EntityMetaData metadata = world.AddOrGet<EntityMetaData>(entity.Entity, new EntityMetaData());
        metadata.LifeStage = EntityLifeStage.Created;
        metadata.ComponentLifeStages = new Dictionary<Type, ComponentLifeStage>();

        entityEventSystem.RaiseEvent(new EntityCreatedEvent(entity));
        logger.LogVerbose($"Entity created: {entity.Id}");
        return entity;
    }

    public void Destroy(EntityRef entity)
    {
        logger.LogVerbose($"Entity marked for destruction: {entity.Id}");

        ref EntityMetaData metadata = ref world.Get<EntityMetaData>(entity.Entity);
        metadata.LifeStage = EntityLifeStage.Terminating;

        // Remove all components
        foreach (var component in world.GetAllComponents(entity.Entity))
        {
            if (component is EntityMetaData) { continue; }
            RemoveComponent(entity, component.GetType());
        }

        entityEventSystem.RaiseEvent(new EntityDestroyedEvent(entity));
    }

    public void AddComponent<T>(EntityRef entity, in T component) where T : IComponent
    {
        EntityMetaData metadata = world.Get<EntityMetaData>(entity.Entity);

        metadata.ComponentLifeStages[typeof(T)] = ComponentLifeStage.Adding;
        world.Add(entity.Entity, component);
        entityEventSystem.RaiseEvent<ComponentAddedEvent, T>(entity, new ComponentAddedEvent());
        metadata.ComponentLifeStages[typeof(T)] = ComponentLifeStage.Added;

        logger.LogVerbose($"{typeof(T).Name} added. (Entity = {entity.Id})");
    }

    public void AddComponent(EntityRef entity, in IComponent component)
    {
        EntityMetaData metadata = world.Get<EntityMetaData>(entity.Entity);

        metadata.ComponentLifeStages[component.GetType()] = ComponentLifeStage.Adding;
        world.Add(entity.Entity, (object)component);
        entityEventSystem.RaiseEvent(entity, component, new ComponentAddedEvent());
        metadata.ComponentLifeStages[component.GetType()] = ComponentLifeStage.Added;

        logger.LogVerbose($"{component.GetType().Name} added. (Entity = {entity.Id})");
    }

    public ref T AddOrGetComponent<T>(EntityRef entity) where T : IComponent, new()
    {
        if (!world.Has<T>(entity.Entity))
        {
            AddComponent(entity, new T());
        }

        return ref world.Get<T>(entity.Entity);
    }

    public void RemoveComponent<T>(EntityRef entity) where T : IComponent
    {
        RemoveComponent(entity, typeof(T));
    }

    public void RemoveComponent(EntityRef entity, Type type)
    {
        logger.LogVerbose($"{type.Name} marked for removal. (Entity = {entity.Id})");

        ref EntityMetaData metadata = ref world.Get<EntityMetaData>(entity.Entity);
        metadata.ComponentLifeStages[type] = ComponentLifeStage.Stopping;
        entityEventSystem.RaiseEvent(new EntityRef(this, entity.Entity), new ComponentStopEvent());
        metadata.ComponentLifeStages[type] = ComponentLifeStage.Stopped;
    }

    public void SetComponent<T>(EntityRef entity, in T component) where T : IComponent
    {
        world.Set(entity.Entity, component);
    }

    public void SetComponent(EntityRef entity, in object component)
    {
        world.Set(entity.Entity, component);
    }

    public ref T GetComponent<T>(EntityRef entity) where T : IComponent
    {
        if (!HasComponent<T>(entity))
        {
            throw new InvalidOperationException($"Entity {entity.Id} does not have component {typeof(T).Name}");
        }

        return ref world.Get<T>(entity.Entity);
    }

    public bool TryGetComponent<T>(EntityRef entity, out T component) where T : IComponent
    {
        return world.TryGet(entity.Entity, out component);
    }

    public ref T TryGetRefComponent<T>(EntityRef entity, out bool exists) where T : IComponent
    {
        return ref world.TryGetRef<T>(entity.Entity, out exists);
    }

    public bool HasComponent<T>(EntityRef entity) where T : IComponent
    {
        return world.Has<T>(entity.Entity);
    }

    public object[] GetComponents(EntityRef entity)
    {
        return world.GetAllComponents(entity.Entity);
    }

    public QueryDescription CreateQuery()
    {
        QueryDescription query = new QueryDescription();
        return query;
    }

    public void Query(in QueryDescription query, ForEachWithEntity action)
    {
        world.Query(query, (Entity entity, ref EntityMetaData metadata) =>
        {
            if (metadata.LifeStage != EntityLifeStage.Created) { return; }
            action(new EntityRef(this, entity));
        });
    }

    public void Query<T1>(in QueryDescription query, ForEachWithEntity<T1> action)
    {
        world.Query(query, (Entity entity, ref EntityMetaData metadata, ref T1 c1) =>
        {
            if (metadata.LifeStage != EntityLifeStage.Created) { return; }
            action(new EntityRef(this, entity), ref c1);
        });
    }

    public void Query<T1, T2>(in QueryDescription query, ForEachWithEntity<T1, T2> action)
    {
        world.Query(query, (Entity entity, ref EntityMetaData metadata, ref T1 c1, ref T2 c2) =>
        {
            if (metadata.LifeStage != EntityLifeStage.Created) { return; }
            action(new EntityRef(this, entity), ref c1, ref c2);
        });
    }

    public void Query<T1, T2, T3>(in QueryDescription query, ForEachWithEntity<T1, T2, T3> action)
    {
        world.Query(query, (Entity entity, ref EntityMetaData metadata, ref T1 c1, ref T2 c2, ref T3 c3) =>
        {
            if (metadata.LifeStage != EntityLifeStage.Created) { return; }
            action(new EntityRef(this, entity), ref c1, ref c2, ref c3);
        });
    }

    public void Query<T1, T2, T3, T4>(in QueryDescription query, ForEachWithEntity<T1, T2, T3, T4> action)
    {
        world.Query(query, (Entity entity, ref EntityMetaData metadata, ref T1 c1, ref T2 c2, ref T3 c3, ref T4 c4) =>
        {
            if (metadata.LifeStage != EntityLifeStage.Created) { return; }
            action(new EntityRef(this, entity), ref c1, ref c2, ref c3, ref c4);
        });
    }

    public void Query<T1, T2, T3, T4, T5>(in QueryDescription query, ForEachWithEntity<T1, T2, T3, T4, T5> action)
    {
        world.Query(query, (Entity entity, ref EntityMetaData metadata, ref T1 c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5) =>
        {
            if (metadata.LifeStage != EntityLifeStage.Created) { return; }
            action(new EntityRef(this, entity), ref c1, ref c2, ref c3, ref c4, ref c5);
        });
    }

    public bool IsValid(EntityRef entity)
    {
        return entity.Valid && world.IsAlive(entity.Entity);
    }

    public bool AnyWith<T>() where T : IComponent
    {
        bool result = false;
        world.Query(new QueryDescription().WithAll<T>(), (Entity entity) =>
        {
            result = true;
        });

        return result;
    }

    internal void Update()
    {
        QueryDescription query = new QueryDescription().WithAll<EntityMetaData>();

        List<Entity> entitiesToProcess = new List<Entity>();

        world.Query(query, (Entity entity) =>
        {
            entitiesToProcess.Add(entity);
        });

        foreach (Entity entity in entitiesToProcess)
        {
            ref EntityMetaData metadata = ref world.Get<EntityMetaData>(entity);

            switch (metadata.LifeStage)
            {
                case EntityLifeStage.Terminating:
                    // Check if all components have been Deleted
                    bool allComponentsDeleted = true;
                    foreach (var stage in metadata.ComponentLifeStages.Values)
                    {
                        if (stage != ComponentLifeStage.Deleted)
                        {
                            allComponentsDeleted = false;
                            break;
                        }
                    }

                    if (allComponentsDeleted)
                    {
                        metadata.LifeStage = EntityLifeStage.Deleted;
                        world.Destroy(entity);
                        logger.LogVerbose($"Entity destroyed: {entity.Id}");
                    }
                    break;
            }

            foreach ((Type component, ComponentLifeStage stage) in metadata.ComponentLifeStages)
            {
                switch (stage)
                {
                    case ComponentLifeStage.Added:
                        metadata.ComponentLifeStages[component] = ComponentLifeStage.Initializing;
                        entityEventSystem.RaiseEvent(new EntityRef(this, entity), new ComponentInitEvent());
                        metadata.ComponentLifeStages[component] = ComponentLifeStage.Initialized;
                        logger.LogVerbose($"{component.Name} initialized. (Entity = {entity.Id})");
                        break;
                    case ComponentLifeStage.Initialized:
                        metadata.ComponentLifeStages[component] = ComponentLifeStage.Starting;
                        entityEventSystem.RaiseEvent(new EntityRef(this, entity), new ComponentStartEvent());
                        metadata.ComponentLifeStages[component] = ComponentLifeStage.Running;
                        logger.LogVerbose($"{component.Name} running. (Entity = {entity.Id})");
                        break;
                    case ComponentLifeStage.Stopped:
                        metadata.ComponentLifeStages[component] = ComponentLifeStage.Removing;
                        entityEventSystem.RaiseEvent(new EntityRef(this, entity), new ComponentRemovedEvent());
                        world.Remove(entity, component);
                        metadata.ComponentLifeStages[component] = ComponentLifeStage.Deleted;
                        logger.LogVerbose($"{component.Name} deleted. (Entity = {entity.Id})");
                        break;
                }
            }
        }
    }
}

public struct EntityRef
{
    public readonly static EntityRef Null = default;

    public int Id => Entity.Entity.Id;
    public int Version => Entity.Version;

    internal EntityReference Entity;
    internal bool Valid = false;

    private WorldRef worldRef;

    internal EntityRef(WorldRef world, Entity entity)
    {
        worldRef = world;
        Entity = entity.Reference();
        Valid = true;
    }

    public void Destroy()
    {
        worldRef.Destroy(this);
    }

    public void Add<T>(in T component) where T : IComponent
    {
        worldRef.AddComponent(this, component);
    }

    public void Add(IComponent component)
    {
        worldRef.AddComponent(this, component);
    }

    public ref T AddOrGet<T>() where T : IComponent, new()
    {
        return ref worldRef.AddOrGetComponent<T>(this);
    }

    public void Remove<T>() where T : IComponent
    {
        worldRef.RemoveComponent<T>(this);
    }

    public void Set<T>(in T component) where T : IComponent
    {
        worldRef.SetComponent(this, component);
    }

    public void Set(object component)
    {
        worldRef.SetComponent(this, component);
    }

    public bool Has<T>() where T : IComponent
    {
        return worldRef.HasComponent<T>(this);
    }

    public ref T Get<T>() where T : IComponent
    {
        return ref worldRef.GetComponent<T>(this);
    }

    public bool TryGet<T>(out T component) where T : IComponent
    {
        return worldRef.TryGetComponent(this, out component);
    }

    public ref T TryGetRef<T>(out bool exists) where T : IComponent
    {
        return ref worldRef.TryGetRefComponent<T>(this, out exists);
    }

    public object[] GetComponents()
    {
        return worldRef.GetComponents(this);
    }

    public bool IsValid()
    {
        return Valid && worldRef.IsValid(this);
    }

    public bool Equals(EntityReference other)
    {
        return Entity.Equals(other.Entity);
    }

    public override bool Equals(object? obj)
    {
        return obj is EntityRef other && Entity.Equals(other.Entity);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Entity.GetHashCode();
        }
    }

    public static bool operator ==(EntityRef left, EntityRef right)
    {
        return left.Entity.Equals(right.Entity);
    }

    public static bool operator !=(EntityRef left, EntityRef right)
    {
        return !left.Entity.Equals(right.Entity);
    }
}