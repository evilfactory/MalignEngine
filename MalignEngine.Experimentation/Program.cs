using MalignEngine;

namespace MalignEngine.Experimentation;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();
        application.Add<EventLoop>();
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
        application.Add<Experimentation>();
        application.Add<ImGuiService>();
        application.Add<FontRenderer>();
        application.Add<PerformanceProfiler>();
        application.Add<EditorSystem>();
        application.Add<EditorConsole>();
        application.Add<EditorSceneViewSystem>();
        application.Add<EditorInspectorSystem>();
        application.Add<EditorPerformanceSystem>();

        application.Run();
    }

}