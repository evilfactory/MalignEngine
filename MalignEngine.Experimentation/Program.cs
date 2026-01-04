using MalignEngine.Editor;

namespace MalignEngine.Experimentation;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();

        // Core services
        application.ServiceContainer.RegisterAll<WindowService>();
        application.ServiceContainer.RegisterAll<GLRenderingAPI>();
        application.ServiceContainer.RegisterAll<Renderer2D>();
        application.ServiceContainer.RegisterAll<FontRenderer>();
        application.ServiceContainer.RegisterAll<EventService>();
        application.ServiceContainer.RegisterAll<InputService>();
        application.ServiceContainer.RegisterAll<EntitySerializer>();

        // Assets
        application.ServiceContainer.RegisterAll<FontAssetLoader>();
        application.ServiceContainer.RegisterAll<AssetService>();
        application.ServiceContainer.RegisterAll<TextureAssetLoader>();
        application.ServiceContainer.RegisterAll<PerformanceProfiler>();
        application.ServiceContainer.RegisterAll<XmlSerializer>();
        application.ServiceContainer.RegisterAll<XmlAssetLoader>();
        application.ServiceContainer.RegisterAll<SpriteXmlAssetLoader>();
        application.ServiceContainer.RegisterAll<TileListXmlLoader>();
        application.ServiceContainer.RegisterAll<SceneXmlLoader>();

        var entityManager = new EntityManager(new ServiceContainer(application.ServiceContainer), application.ServiceContainer.GetInstance<IScheduleManager>());

        // World
        entityManager.WorldContainer.RegisterAll<CameraSystem>();
        entityManager.WorldContainer.RegisterAll<TransformSystem>();
        entityManager.WorldContainer.RegisterAll<HierarchySystem>();
        entityManager.WorldContainer.RegisterAll<SpriteRenderingSystem>();
        entityManager.WorldContainer.RegisterAll<SceneSystem>();
        entityManager.WorldContainer.RegisterAll<PhysicsSystem2D>();
        entityManager.WorldContainer.RegisterAll<Experimentation>();

        application.ServiceContainer.RegisterAll<ImGuiSystem>();
        application.ServiceContainer.RegisterAll<EditorSystem>();
        application.ServiceContainer.RegisterAll<EditorConsole>();
        application.ServiceContainer.RegisterAll<EditorPerformanceSystem>();
        application.ServiceContainer.RegisterAll<EditorAssetViewer>();
        entityManager.WorldContainer.RegisterAll<EditorSceneViewSystem>();
        entityManager.WorldContainer.RegisterAll<EditorInspectorSystem>();


        application.Run();
    }

}