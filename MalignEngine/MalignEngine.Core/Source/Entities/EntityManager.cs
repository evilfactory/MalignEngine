using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MalignEngine;

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
}

[Stage<IPostUpdate, HighestPriorityStage>]
public sealed class EntityManager : IEntityManager, IDisposable, IApplicationRun, IPostUpdate
{
    public World World { get; init; }
    public IServiceContainer WorldContainer { get; init; }

    private Queue<Entity> backDestroyBuffer;
    private Queue<Entity> frontDestroyBuffer;

    private readonly IScheduleManager _scheduleManager;

    private bool disposing = false;

    public EntityManager(IServiceContainer worldContainer, IScheduleManager scheduleManager)
    {
        _scheduleManager = scheduleManager;

        World = new World();
        WorldContainer = worldContainer;

        WorldContainer.Register<IEntityManager, EntityManager>(new SingletonLifeTime(this));
        WorldContainer.Register<IWorld, World>(new SingletonLifeTime(World));

        backDestroyBuffer = new Queue<Entity>();
        frontDestroyBuffer = new Queue<Entity>();

        _scheduleManager.RegisterAll(this);
    }

    public void Destroy(Entity entity)
    {
        World.AddOrSetComponent(entity, new Destroyed());

        frontDestroyBuffer.Enqueue(entity);
    }

    public void OnApplicationRun()
    {
        WorldContainer.GetInstance<IEnumerable<ISystem>>();
    }

    public void OnPostUpdate(float deltaTime)
    {
        while (backDestroyBuffer.TryDequeue(out Entity entity))
        {
            if (!World.IsAlive(entity)) { continue; }

            World.DestroyImmediate(entity);
        }

        // Swap buffers
        var temp = backDestroyBuffer;
        backDestroyBuffer = frontDestroyBuffer;
        frontDestroyBuffer = temp;
    }

    public void Dispose()
    {
        if (disposing) { return; }

        disposing = true;

        _scheduleManager.UnregisterAll(this);
        WorldContainer.Dispose();
    }
}
