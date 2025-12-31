using MalignEngine.Editor;

namespace MalignEngine.Experimentation;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();
        application.Add<WindowService>();
        application.Add<EntityManager>();
        application.Add<EventService>();
        application.Add<CameraSystem>();
        application.Add<TransformSystem>();
        application.Add<ParentSystem>();
        application.Add<SpriteRenderingSystem>();
        application.Add<GLRenderingAPI>();
        application.Add<InputService>();
        application.Add<Renderer2D>();
        application.Add<XmlSerializer>();
        application.Add<EntitySerializer>();
        application.Add<SceneSystem>();
        application.Add<SceneXmlLoader>();
        application.Add<PhysicsSystem2D>();
        application.Add<Experimentation>();
        application.Add<ImGuiSystem>();
        application.Add<FontRenderer>();
        application.Add<FontAssetLoader>();
        application.Add<AssetService>();
        application.Add<TextureAssetLoader>();
        application.Add<PerformanceProfiler>();
        application.Add<XmlAssetLoader>();
        application.Add<SpriteXmlAssetLoader>();
        application.Add<TileListXmlLoader>();
        application.Add<EditorSystem>();
        application.Add<EditorConsole>();
        application.Add<EditorSceneViewSystem>();
        application.Add<EditorInspectorSystem>();
        application.Add<EditorPerformanceSystem>();
        application.Add<EditorAssetViewer>();
        application.Add<EditorTile>();
        application.Add<TileSystem>();

        application.Run();
    }

}