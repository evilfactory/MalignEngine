using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MalignEngine;

public interface ICommitWorldChanges : ISchedule
{
    void CommitWorldChanges();
}

public interface IEntityCommand
{
    void Execute(World world);
}

public record struct CreateEntityCommand(Entity Entity) : IEntityCommand
{
    public void Execute(World world)
    {
        world.CreateEntity();
    }
}

public record struct AddOrSetComponentCommand(Entity Entity, IComponent Component) : IEntityCommand
{
    public void Execute(World world)
    {
        world.AddOrSetComponent(Entity, Component);
    }
}

public record struct DestroyEntityCommand(Entity Entity) : IEntityCommand
{
    public void Execute(World world)
    {
        world.DestroyImmediate(Entity);
    }
}

/// <summary>
/// World subsystem, keeps a container that stores a world and all services running on it
/// </summary>
public interface IEntityManager
{
    public World World { get; }
    public IServiceContainer WorldContainer { get; }

    /// <summary>
    /// Marks the entity for destruction.
    /// </summary>
    public void Destroy(Entity entity);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    public Entity Create();

    /// <summary>
    /// Adds or sets the component in an entity.
    /// </summary>
    public void AddOrSetComponent(Entity entity, IComponent component);

    public void Query(Query query, Action<Entity> forEach);
    public bool HasComponent(Entity entity, Type type);
    public bool HasComponent<T>(Entity entity) where T : IComponent;
    public IComponent GetComponent(Entity entity, Type type);
    public bool TryGetComponent(Entity entity, Type type, [NotNullWhen(returnValue: true)] out IComponent? component);
    public bool TryGetComponent<T>(Entity entity, [NotNullWhen(returnValue: true)] out ComponentRef<T> component) where T : IComponent;
}

public sealed class EntityManager : IEntityManager, IDisposable, IApplicationRun, ICommitWorldChanges
{
    public World World { get; init; }
    public IServiceContainer WorldContainer { get; init; }

    private Queue<IEntityCommand> _commandBuffer = [];

    private readonly IScheduleManager _scheduleManager;

    private bool disposing = false;

    public EntityManager(IServiceContainer worldContainer, IScheduleManager scheduleManager)
    {
        _scheduleManager = scheduleManager;

        World = new World();
        WorldContainer = worldContainer;

        WorldContainer.Register<IEntityManager, EntityManager>(new SingletonLifeTime(this));
        WorldContainer.Register<IWorld, World>(new SingletonLifeTime(World));

        _scheduleManager.RegisterAll(this);
    }

    public Entity Create()
    {
        return World.CreateEntity();
    }

    public void Destroy(Entity entity)
    {
        _commandBuffer.Enqueue(new DestroyEntityCommand(entity));
    }

    public void AddOrSetComponent(Entity entity, IComponent component)
    {
        _commandBuffer.Enqueue(new AddOrSetComponentCommand(entity, component));
    }

    public void Query(Query query, Action<Entity> forEach) => World.Query(query, forEach);
    public bool HasComponent(Entity entity, Type type) => World.HasComponent(entity, type);
    public bool HasComponent<T>(Entity entity) where T : IComponent => World.HasComponent<T>(entity);
    public IComponent GetComponent(Entity entity, Type type) => World.GetComponent(entity, type);
    public bool TryGetComponent(Entity entity, Type type, [NotNullWhen(returnValue: true)] out IComponent? component) => World.TryGetComponent(entity, type, out component);
    public bool TryGetComponent<T>(Entity entity, [NotNullWhen(returnValue: true)] out ComponentRef<T> component) where T : IComponent => World.TryGetComponent<T>(entity, out component);

    public void OnApplicationRun()
    {
        WorldContainer.GetInstance<IEnumerable<ISystem>>();
    }

    public void CommitWorldChanges()
    {
        while (_commandBuffer.TryDequeue(out IEntityCommand? command))
        {
            command.Execute(World);
        }
    }

    public void Dispose()
    {
        if (disposing) { return; }

        disposing = true;

        _scheduleManager.UnregisterAll(this);
        WorldContainer.Dispose();
    }
}
