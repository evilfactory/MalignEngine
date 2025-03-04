using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using System.Linq;

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

public class EntityCreatedEvent : EventArgs
{
    public EntityRef Entity;

    public EntityCreatedEvent(EntityRef entity)
    {
        Entity = entity;
    }
}
public class EntityDestroyedEvent : EventArgs
{
    public EntityRef Entity;

    public EntityDestroyedEvent(EntityRef entity)
    {
        Entity = entity;
    }
}

public class ComponentAddedEvent : ComponentEventArgs { }
public class ComponentInitEvent : ComponentEventArgs { }
public class ComponentStartEvent : ComponentEventArgs { }
public class ComponentStopEvent : ComponentEventArgs { }
public class ComponentRemovedEvent : ComponentEventArgs { }
public class ComponentDeletedEvent : ComponentEventArgs { }

public delegate void ForEachWithEntity(EntityRef entity);
public delegate void ForEachWithEntity<T0>(EntityRef entity, ref T0 t0Component);
public delegate void ForEachWithEntity<T0, T1>(EntityRef entity, ref T0 t0Component, ref T1 t1Component);
public delegate void ForEachWithEntity<T0, T1, T2>(EntityRef entity, ref T0 t0Component, ref T1 t1Component, ref T2 t2Component);
public delegate void ForEachWithEntity<T0, T1, T2, T3>(EntityRef entity, ref T0 t0Component, ref T1 t1Component, ref T2 t2Component, ref T3 t3Component);
public delegate void ForEachWithEntity<T0, T1, T2, T3, T4>(EntityRef entity, ref T0 t0Component, ref T1 t1Component, ref T2 t2Component, ref T3 t3Component, ref T4 t4Component);

public abstract class ComponentEventArgs : EventArgs { }

public class ComponentEventChannel<T> : IEventChannel<T> where T : ComponentEventArgs
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

    private List<EventSubscription> eventSubscriptions = new();

    public void Raise(EntityRef entity, T args)
    {
        var componentTypes = entity.GetComponents().ToList().Select(x => x.GetType());

        foreach (var subscription in eventSubscriptions)
        {
            if (subscription.Handler is Action<EntityRef, T> handler && componentTypes.Contains(subscription.ComponentType))
            {
                handler(entity, args);
            }
        }
    }

    public void Raise<TComponent>(EntityRef entity, T args)
    {
        foreach (var subscription in eventSubscriptions)
        {
            if (subscription.Handler is Action<EntityRef, T> handler && typeof(TComponent) == subscription.ComponentType)
            {
                handler(entity, args);
            }
        }
    }

    public void Raise(EntityRef entity, Type componentType, T args)
    {
        foreach (var subscription in eventSubscriptions)
        {
            if (subscription.Handler is Action<EntityRef, T> handler && componentType == subscription.ComponentType)
            {
                handler(entity, args);
            }
        }
    }

    public void Subscribe<TComponent>(Action<EntityRef, T> handler)
    {
        eventSubscriptions.Add(new EventSubscription(handler, typeof(TComponent)));
    }
}

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

    private ILogger Logger;
    private EventService eventService;

    public WorldRef World
    {
        get => world;
    }

    private WorldRef world = default!;

    public EntityManagerService(ILoggerService LoggerService, EventService eventService)
    {
        Logger = LoggerService.GetSawmill("ents");

        eventService.Register(new EventChannel<EntityCreatedEvent>());
        eventService.Register(new EventChannel<EntityDestroyedEvent>());
        eventService.Register(new ComponentEventChannel<ComponentAddedEvent>());
        eventService.Register(new ComponentEventChannel<ComponentInitEvent>());
        eventService.Register(new ComponentEventChannel<ComponentStartEvent>());
        eventService.Register(new ComponentEventChannel<ComponentStopEvent>());
        eventService.Register(new ComponentEventChannel<ComponentRemovedEvent>());
        eventService.Register(new ComponentEventChannel<ComponentDeletedEvent>());

        this.eventService = eventService;
    }

    public void OnInitialize()
    {
        world = new WorldRef(Logger, eventService);
    }

    public void OnPostUpdate(float deltaTime)
    {
        World.Update();
    }
}
