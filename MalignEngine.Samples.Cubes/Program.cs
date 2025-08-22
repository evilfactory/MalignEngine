using MalignEngine;

namespace MalignEngine.Samples.Cubes;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();
        application.Add<EventLoop>();
        application.Add<WindowService>();
        application.Add<GLRenderingAPI>();
        application.Add<DrawLoopBefore>();
        application.Add<DrawLoopAfter>();
        application.Add<InputService>();
        application.Add<Renderer2D>();
        application.Add<Cubes>();
        application.Add<ImGuiService>();
        application.Add<PerformanceProfiler>();
        application.Add<EditorSystem>();
        application.Add<EditorPerformanceSystem>();

        application.Run();
    }

}