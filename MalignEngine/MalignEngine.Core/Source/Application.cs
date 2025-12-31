using Arch.Core;
using System.Globalization;
using System.Reflection;

namespace MalignEngine;

public class Application : IDisposable, ILogHandler
{
    public static Application? Main { get; private set; }

    public ServiceContainer ServiceContainer { get; private set; }
    public ScheduleManager ScheduleManager => ServiceContainer.GetInstance<ScheduleManager>();
    public StateManager StateManager => ServiceContainer.GetInstance<StateManager>();

    public Application()
    {
        Main = this;

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        ServiceContainer = new ServiceContainer();

        ServiceContainer.Register<IServiceContainer, ServiceContainer>(new SingletonLifeTime(ServiceContainer));

        ServiceContainer.RegisterAll<LoggerService>(new SingletonLifeTime());
        ServiceContainer.RegisterAll<ScheduleManager>(new SingletonLifeTime());
        ServiceContainer.RegisterAll<StateManager>(new SingletonLifeTime());

        ServiceContainer.GetInstance<LoggerService>().Root.AddHandler(this);

        ServiceContainer.GetInstance<LoggerService>().LogInfo("Hello!");
    }

    public void Add(ServiceSet serviceSet)
    {
        serviceSet.Put(this);
    }

    public void Add<T>(ILifeTime? lifetime = null)
    {
        ServiceContainer.RegisterAll<T>(lifetime);
    }

    /// <summary>
    /// Registers scheduler subscribers and Executes the IApplicationRun schedule
    /// </summary>
    public void Run()
    {
        ServiceContainer.GetInstance<ScheduleManager>().Run<IApplicationRun>(e => e.OnApplicationRun());

        Dispose();
        ServiceContainer.GetInstance<LoggerService>().LogInfo("Goodbye!");
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
}