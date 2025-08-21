namespace MalignEngine.Samples.Minesweeper;

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
        application.Add<PerformanceProfiler>();
        application.Add<Minesweeper>();
        application.Add<EditorSystem>();

        application.Run();
    }

}