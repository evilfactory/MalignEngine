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

public abstract class EntityEventArgs { }

public sealed class EntityManagerService : IService, IInit, IPostUpdate
{
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

    [Dependency]
    private ILoggerService LoggerService;

    private ILogger Logger;

    public WorldRef World
    {
        get => world;
    }

    private WorldRef world = default!;

    public void OnInitialize()
    {
        Logger = LoggerService.GetSawmill("ents");

        world = new WorldRef(Logger, this);
    }

    public void OnPostUpdate(float deltaTime)
    {
        World.Update();
    }

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
        HashSet<Type> componentTypes = World.GetComponents(entity).Select(comp => comp.GetType()).ToHashSet();

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

    public void RaiseEvent<TEvent>(EntityRef entity, IComponent component, TEvent args) where TEvent : EntityEventArgs
    {
        foreach (var subscription in eventSubscriptions)
        {
            if (subscription.Handler is Action<EntityRef, TEvent> handler && subscription.ComponentType == component.GetType())
            {
                handler(entity, args);
            }
        }
    }
}
