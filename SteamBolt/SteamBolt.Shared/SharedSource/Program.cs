using MalignEngine;
using MalignEngine.Network;
using System.Reflection;

namespace SteamBolt;

internal class Program
{
    static void Main(string[] args)
    {
        Application application = new DesktopApplication();

        // Core services
#if CLIENT
        application.ServiceContainer.RegisterAll<WindowService>();
        application.ServiceContainer.RegisterAll<GLRenderingAPI>();
        application.ServiceContainer.RegisterAll<Renderer2D>();
        application.ServiceContainer.RegisterAll<FontRenderer>();
        application.ServiceContainer.RegisterAll<InputService>();
#endif
        application.ServiceContainer.RegisterAll<EntitySerializer>();

        // Assets
        application.ServiceContainer.RegisterAll<FontAssetLoader>();
        application.ServiceContainer.RegisterAll<AssetService>();
        application.ServiceContainer.RegisterAll<PerformanceProfiler>();
        application.ServiceContainer.RegisterAll<XmlSerializer>();
        application.ServiceContainer.RegisterAll<TileListXmlLoader>();
        application.ServiceContainer.RegisterAll<SceneXmlLoader>();
        application.ServiceContainer.RegisterAll<FixtureDataFieldSerializer>();

        // Networking
#if SERVER
        application.ServiceContainer.RegisterAll<ServerSessionSystem>();
        application.ServiceContainer.RegisterAll<DummySpriteXmlAssetLoader>();
        application.ServiceContainer.RegisterAll<NetworkServer>();
#elif CLIENT
        application.ServiceContainer.RegisterAll<ClientSessionSystem>();
        application.ServiceContainer.RegisterAll<SpriteXmlAssetLoader>();
        application.ServiceContainer.RegisterAll<TextureAssetLoader>();
        application.ServiceContainer.RegisterAll<NetworkClient>();
#endif

        var entityManager = new EntityManager(new ServiceContainer(application.ServiceContainer), application.ServiceContainer.GetInstance<IScheduleManager>());

        // World
#if CLIENT
        entityManager.WorldContainer.RegisterAll<CameraSystem>();
        entityManager.WorldContainer.RegisterAll<SpriteRenderingSystem>();

#endif
        entityManager.WorldContainer.RegisterAll<EventService>();
        entityManager.WorldContainer.RegisterAll<EntityNetworkSystem>();
        entityManager.WorldContainer.RegisterAll<NetworkService>();
        entityManager.WorldContainer.RegisterAll<ReplicationSystem>();
        entityManager.WorldContainer.RegisterAll<TransformReplicator>();

        entityManager.WorldContainer.RegisterAll<TransformSystem>();
        entityManager.WorldContainer.RegisterAll<HierarchySystem>();
        entityManager.WorldContainer.RegisterAll<SceneSystem>();
        entityManager.WorldContainer.RegisterAll<PhysicsSystem2D>();

#if CLIENT
        MalignEngine.Editor.Defaults.Editor(application, entityManager);
#endif


#if CLIENT
        EventLoop eventLoop = new EventLoop(
            application.ServiceContainer.GetInstance<IScheduleManager>(),
            new ExecutionPipeline()
                .Stage<IPreUpdate>((s, c) => s.OnPreUpdate((float)c.DeltaTime))
                .Stage<IUpdate>((s, c) => s.OnUpdate((float)c.DeltaTime))
                .Stage<PhysicsStep>((s, c) => s.DoPhysicsStep((float)c.DeltaTime))
                .Stage<IPostUpdate>((s, c) => s.OnPostUpdate((float)c.DeltaTime))
                .Stage<ICommitWorldChanges>((s, c) => s.CommitWorldChanges()),

            new ExecutionPipeline()
                .Stage<IBeginFrame>((s, c) => s.BeginFrame())
                .Stage<IPreDraw>((s, c) => s.OnPreDraw((float)c.DeltaTime))
                .Stage<IDraw>((s, c) => s.OnDraw((float)c.DeltaTime))
                .Stage<IPostDraw>((s, c) => s.OnPostDraw((float)c.DeltaTime))
                .Stage<IEndFrame>((s, c) => s.EndFrame())
            );
#elif SERVER
        EventLoop eventLoop = new EventLoop(
            application.ServiceContainer.GetInstance<IScheduleManager>(),
            new ExecutionPipeline()
                .Stage<IPreUpdate>((s, c) => s.OnPreUpdate((float)c.DeltaTime))
                .Stage<IUpdate>((s, c) => s.OnUpdate((float)c.DeltaTime))
                .Stage<IPostUpdate>((s, c) => s.OnPostUpdate((float)c.DeltaTime))
                .Stage<PhysicsStep>((s, c) => s.DoPhysicsStep((float)c.DeltaTime)),
            drawPipeline: null
        );
#endif

        application.ServiceContainer.Register<IEventLoop, EventLoop>(new SingletonLifeTime(eventLoop));

        application.ServiceContainer.RegisterAssembly(Assembly.GetExecutingAssembly(), [typeof(IService)], [typeof(IEntitySystem)]);
        entityManager.WorldContainer.RegisterAssembly(Assembly.GetExecutingAssembly(), [typeof(IEntitySystem)], []);

        application.Initialize();
        eventLoop.Run();
    }
}