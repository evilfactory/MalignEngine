using System.Globalization;
using System.Reflection;

namespace MalignEngine;

public class DesktopApplication : Application, IApplicationClosing, ILogHandler, IDisposable
{
    public override IServiceContainer ServiceContainer { get; protected set; }

    public DesktopApplication() : base()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        ServiceContainer = new ServiceContainer();

        ServiceContainer.Register<ILoggerService, LoggerService>(new SingletonLifeTime());
        ServiceContainer.Register<IScheduleManager, ScheduleManager>(new SingletonLifeTime());

        ServiceContainer.GetInstance<IScheduleManager>().Register<IApplicationClosing>(this);

        ServiceContainer.GetInstance<ILoggerService>().Root.AddHandler(this);
        ServiceContainer.GetInstance<ILoggerService>().LogInfo("Hello!");
    }

    /// <summary>
    /// Instantiates all systems and creates the event loop
    /// </summary>
    public override void Initialize()
    {
        ServiceContainer.GetInstance<IEnumerable<ISystem>>(); // Instantiate all systems

        ServiceContainer.GetInstance<IScheduleManager>().Run<IApplicationRun>(x => x.OnApplicationRun());
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
        ServiceContainer.GetInstance<ILoggerService>().LogInfo("Goodbye!");

        ServiceContainer.Dispose();
    }

    public void OnApplicationClosing()
    {
        Dispose();
    }
}