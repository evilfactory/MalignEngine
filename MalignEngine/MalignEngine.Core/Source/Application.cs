using System.Globalization;
using System.Reflection;

namespace MalignEngine;

public class Application : IDisposable, ILogHandler, IApplicationClosing
{
    public static Application? Main { get; private set; }

    public IEventLoop? EventLoop { get; private set; }
    public IServiceContainer ServiceContainer { get; private set; }

    public Application()
    {
        Main = this;

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        ServiceContainer = new ServiceContainer();

        ServiceContainer.Register<IServiceContainer, ServiceContainer>(new SingletonLifeTime(ServiceContainer));
        ServiceContainer.Register<ILoggerService, LoggerService>(new SingletonLifeTime());
        ServiceContainer.Register<IScheduleManager, ScheduleManager>(new SingletonLifeTime());

        ServiceContainer.GetInstance<IScheduleManager>().Register<IApplicationClosing>(this);

        ServiceContainer.GetInstance<ILoggerService>().Root.AddHandler(this);
        ServiceContainer.GetInstance<ILoggerService>().LogInfo("Hello!");
    }

    /// <summary>
    /// Instantiates all systems and creates the event loop
    /// </summary>
    public void Run()
    {
        ServiceContainer.TryGetInstance(out IPerformanceProfiler? performanceProfiler);
        EventLoop = new EventLoop(ServiceContainer.GetInstance<IScheduleManager>(), performanceProfiler);

        ServiceContainer.GetInstance<IEnumerable<ISystem>>(); // Instantiate all systems

        ServiceContainer.GetInstance<IScheduleManager>().Run<IApplicationRun>(x => x.OnApplicationRun());

        // Start the event loop
        EventLoop.Run();

        Dispose();
        ServiceContainer.GetInstance<ILoggerService>().LogInfo("Goodbye!");
    }

    public void HandleLog(Sawmill sawmill, LogEvent logEvent)
    {
        switch (logEvent.LogType)
        {
            case LogType.Verbose:
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[VERB] [{sawmill.Name}] {logEvent.Message}");
                break;

            case LogType.Info:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[INFO] [{sawmill.Name}] {logEvent.Message}");
                break;

            case LogType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] [{sawmill.Name}] {logEvent.Message}");
                break;

            case LogType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERRO] [{sawmill.Name}] {logEvent.Message}");
                break;
        }

        Console.ForegroundColor = ConsoleColor.White;
    }

    public void Dispose()
    {
        ServiceContainer.Dispose();
    }

    public void OnApplicationClosing()
    {
        EventLoop?.Stop();
    }
}