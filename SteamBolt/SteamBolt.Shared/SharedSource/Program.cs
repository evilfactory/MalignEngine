using MalignEngine;
using MalignEngine.Editor;
using MalignEngine.Network;

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

        // Networking
#if SERVER
        application.ServiceContainer.RegisterAll<NetworkServer>();
        application.ServiceContainer.RegisterAll<ServerSessionSystem>();
        application.ServiceContainer.RegisterAll<DummySpriteXmlAssetLoader>();
#elif CLIENT
        application.ServiceContainer.RegisterAll<NetworkClient>();
        application.ServiceContainer.RegisterAll<ClientSessionSystem>();
        application.ServiceContainer.RegisterAll<SpriteXmlAssetLoader>();
        application.ServiceContainer.RegisterAll<TextureAssetLoader>();
#endif
        application.ServiceContainer.RegisterAll<SessionHandler>();

        var entityManager = new EntityManager(new ServiceContainer(application.ServiceContainer), application.ServiceContainer.GetInstance<IScheduleManager>());

        // World
#if CLIENT
        entityManager.WorldContainer.RegisterAll<CameraSystem>();
        entityManager.WorldContainer.RegisterAll<SpriteRenderingSystem>();
        entityManager.WorldContainer.RegisterAll<ClientEntityNetworkSystem>();
        entityManager.WorldContainer.RegisterAll<PlayerInputSystem>();
        entityManager.WorldContainer.RegisterAll<PlayerMovementSystem>();
#elif SERVER
        entityManager.WorldContainer.RegisterAll<PlayerSpawnerSystem>();
        entityManager.WorldContainer.RegisterAll<ServerEntityNetworkSystem>();
        entityManager.WorldContainer.RegisterAll<PlayerMovementSystem>();
#endif

        entityManager.WorldContainer.RegisterAll<TransformSystem>();
        entityManager.WorldContainer.RegisterAll<HierarchySystem>();
        entityManager.WorldContainer.RegisterAll<SceneSystem>();
        entityManager.WorldContainer.RegisterAll<PhysicsSystem2D>();
        entityManager.WorldContainer.RegisterAll<SteamBolt>();

#if CLIENT
        application.ServiceContainer.RegisterAll<ImGuiSystem>();
        application.ServiceContainer.RegisterAll<EditorSystem>();
        application.ServiceContainer.RegisterAll<EditorConsole>();
        application.ServiceContainer.RegisterAll<EditorPerformanceSystem>();
        application.ServiceContainer.RegisterAll<EditorAssetViewer>();
        entityManager.WorldContainer.RegisterAll<EditorSceneViewSystem>();
        entityManager.WorldContainer.RegisterAll<EditorInspectorSystem>();
#endif


        application.Run();
    }
}
