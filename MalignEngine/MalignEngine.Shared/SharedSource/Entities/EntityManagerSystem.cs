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
        Logger = LoggerService.GetSawmill("ents");

        world = new WorldRef(Logger, EntityEventSystem);
        IoCManager.Register(world);
    }

    public void OnPostUpdate(float deltaTime)
    {
        World.Update();
    }
}
