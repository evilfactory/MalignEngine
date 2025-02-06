using Arch.Core;
using System.Reflection;

namespace MalignEngine
{
    public class Application : ILogHandler
    {
        public ScheduleManager ScheduleManager => scheduleManager;
        public StateManager StateManager => stateManager;

        private ScheduleManager scheduleManager;
        private StateManager stateManager;
        private LoggerService logger;

        private readonly List<IService> systems;

        public Application()
        {
            systems = new List<IService>();

            scheduleManager = new ScheduleManager();
            Add(scheduleManager);

            stateManager = new StateManager(scheduleManager);
            Add(stateManager);

            logger = new LoggerService();
            logger.Root.AddHandler(this);
            Add(logger);
        }

        public void Add(IService system, ScheduleMetaData? scheduleMetaData = null)
        {
            systems.Add(system);
            scheduleManager.SubscribeAll(system, scheduleMetaData);
            IoCManager.Register(system);
        }

        public void Add(ServiceSet serviceSet)
        {
            serviceSet.Put(this);
        }

        public void Add(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(IService)) && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    Add((IService)Activator.CreateInstance(type));
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
                }
                catch (Exception e)
                {
                    logger.LogError($"Failed to inject dependencies to system {system.GetType().Name}: {e.Message}.");
                }
            }
        }

        /// <summary>
        /// Executes the IApplicationRun schedule
        /// </summary>
        public void Run()
        {
            Initialize();
            IoCManager.Resolve<ScheduleManager>().Run<IApplicationRun>(e => e.OnApplicationRun());
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
    }
}