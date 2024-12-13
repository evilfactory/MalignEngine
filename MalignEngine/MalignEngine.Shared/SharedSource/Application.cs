using Arch.Core;
using System.Reflection;

namespace MalignEngine
{
    public class Application : ILogHandler
    {
        private LoggerService logger;

        private readonly List<IService> systems;

        public Application()
        {
            systems = new List<IService>();

            logger = new LoggerService();
            logger.Root.AddHandler(this);
            AddSystem(logger);

            AddSystem(new EventSystem());
        }

        public void AddSystem(IService system)
        {
            systems.Add(system);
            IoCManager.Register(system);

            logger.LogVerbose($"Added system {system.GetType().Name}");
        }

        public void AddAllSystems(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(IService)) && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    AddSystem((IService)Activator.CreateInstance(type));
                }
            }
        }

        public void Initialize()
        {
            // Log all system dependencies
            foreach (var system in systems)
            {
                var dependencies = IoCManager.GetDependencies(system.GetType());
                if (dependencies.Length > 0)
                {
                    logger.LogVerbose($"System {system.GetType().Name} depends on:");
                    foreach (var dependency in dependencies)
                    {
                        logger.LogVerbose($" - {dependency.Name}");
                    }
                }
                else
                {
                    logger.LogVerbose($"System {system.GetType().Name} depends on nothing.");
                }
            }

            foreach (var system in systems)
            {
                try
                {
                    IoCManager.InjectDependencies(system);
                    IoCManager.Resolve<EventSystem>().SubscribeAll(system);
                }
                catch (Exception e)
                {
                    logger.LogError($"Failed to inject dependencies to system {system.GetType().Name}: {e.Message}.");
                }
            }
        }
        public void Run()
        {
            Initialize();
            IoCManager.Resolve<EventSystem>().PublishEvent<IApplicationRun>(e => e.OnApplicationRun());
        }

        public void HandleLog(Sawmill sawmill, LogEvent logEvent)
        {
            switch (logEvent.LogType)
            {
                case LogType.Verbose:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[VERBOSE] [{sawmill.Name}] {logEvent.Message}");
                    break;

                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"[INFO] [{sawmill.Name}] {logEvent.Message}");
                    break;

                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] [{sawmill.Name}] {logEvent.Message}");
                    break;

                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] [{sawmill.Name}] {logEvent.Message}");
                    break;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}