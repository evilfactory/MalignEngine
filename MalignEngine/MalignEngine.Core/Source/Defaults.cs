using System.Reflection;

namespace MalignEngine;

public static class Defaults
{
    public static IEnumerable<Type> FindImplementations<TInterface>()
    {
        var interfaceType = typeof(TInterface);

        if (!interfaceType.IsInterface)
        {
            throw new ArgumentException($"{interfaceType.Name} is not an interface.");
        }

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    public static void Essentials(Application application)
    {
        application.ServiceContainer.RegisterAll(FindImplementations<IWindowService>().First());
        application.ServiceContainer.RegisterAll(FindImplementations<IRenderingAPI>().First());
        application.ServiceContainer.RegisterAll(FindImplementations<IInputService>().First());
        application.ServiceContainer.RegisterAll<Renderer2D>();
        application.ServiceContainer.RegisterAll<FontRenderer>();
        application.ServiceContainer.RegisterAll<EventService>();
        application.ServiceContainer.RegisterAll<EntitySerializer>();
        application.ServiceContainer.RegisterAll<PerformanceProfiler>();

        application.ServiceContainer.RegisterAll<AssetService>();
        application.ServiceContainer.RegisterAll<FontAssetLoader>();
        application.ServiceContainer.RegisterAll<TextureAssetLoader>();
        application.ServiceContainer.RegisterAll<XmlSerializer>();
        application.ServiceContainer.RegisterAll<SpriteXmlAssetLoader>();
        application.ServiceContainer.RegisterAll<SceneXmlLoader>();
        application.ServiceContainer.RegisterAll<ShaderAssetLoader>();

        application.ServiceContainer.RegisterAll<UIManager>();
        application.ServiceContainer.RegisterAll<UIPainter>();
    }

    public static EntityManager Entity(Application application)
    {
        var entityManager = new EntityManager(new ServiceContainer(application.ServiceContainer), application.ServiceContainer.GetInstance<IScheduleManager>());

        entityManager.WorldContainer.RegisterAll<CameraSystem>();
        entityManager.WorldContainer.RegisterAll<TransformSystem>();
        entityManager.WorldContainer.RegisterAll<HierarchySystem>();
        entityManager.WorldContainer.RegisterAll<SpriteRenderingSystem>();
        entityManager.WorldContainer.RegisterAll<SceneSystem>();
        entityManager.WorldContainer.RegisterAll<PhysicsSystem2D>();

        return entityManager;
    }

    public static EventLoop EventLoop(Application application)
    {
        EventLoop eventLoop = new EventLoop(
            application.ServiceContainer.GetInstance<IScheduleManager>(),
            new ExecutionPipeline()
                .Stage<IPreUpdate>((s, c) => s.OnPreUpdate((float)c.DeltaTime))
                .Stage<IUpdate>((s, c) => s.OnUpdate((float)c.DeltaTime))
                .Stage<IPostUpdate>((s, c) => s.OnPostUpdate((float)c.DeltaTime))
                .Stage<ICommitWorldChanges>((s, c) => s.CommitWorldChanges()),
            new ExecutionPipeline()
                .Stage<IBeginFrame>((s, c) => s.BeginFrame())
                .Stage<IPreDraw>((s, c) => s.OnPreDraw((float)c.DeltaTime))
                .Stage<IDraw>((s, c) => s.OnDraw((float)c.DeltaTime))
                .Stage<IPostDraw>((s, c) => s.OnPostDraw((float)c.DeltaTime))
                .Stage<IEndFrame>((s, c) => s.EndFrame())
        );

        application.ServiceContainer.Register<IEventLoop, EventLoop>(new SingletonLifeTime(eventLoop));

        return eventLoop;
    }
}