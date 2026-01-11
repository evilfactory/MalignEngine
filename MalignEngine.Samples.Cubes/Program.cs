using MalignEngine;
using MalignEngine.Editor;

namespace MalignEngine.Samples.Cubes;

class Program
{
    public static void Main(string[] args)
    {
        Application application = new Application();
        application.ServiceContainer.RegisterAll<WindowService>();
        application.ServiceContainer.RegisterAll<GLRenderingAPI>();
        application.ServiceContainer.RegisterAll<InputService>();
        application.ServiceContainer.RegisterAll<Renderer2D>();
        application.ServiceContainer.RegisterAll<Cubes>();
        application.ServiceContainer.RegisterAll<ImGuiSystem>();
        application.ServiceContainer.RegisterAll<PerformanceProfiler>();
        application.ServiceContainer.RegisterAll<EditorSystem>();
        application.ServiceContainer.RegisterAll<EditorPerformanceSystem>();

        application.Run();
    }

}