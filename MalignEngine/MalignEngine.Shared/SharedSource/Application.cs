using Arch.Core;

namespace MalignEngine
{
    public class Application
    {
        private readonly List<BaseSystem> systems;

        public Application()
        {
            systems = new List<BaseSystem>();

            AddSystem(new EventSystem());
        }

        public void AddSystem(BaseSystem system)
        {
            systems.Add(system);
            IoCManager.Register(system);

            Logger.LogVerbose($"Added system {system.GetType().Name}");
        }

        public void Initialize()
        {
            // Log all system dependencies
            foreach (var system in systems)
            {
                var dependencies = IoCManager.GetDependencies(system.GetType());
                if (dependencies.Length > 0)
                {
                    Logger.LogVerbose($"System {system.GetType().Name} depends on:");
                    foreach (var dependency in dependencies)
                    {
                        Logger.LogVerbose($" - {dependency.Name}");
                    }
                }
                else
                {
                    Logger.LogVerbose($"System {system.GetType().Name} depends on nothing.");
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
                    system.Enabled = false;
                    Logger.LogError($"Failed to inject dependencies to system {system.GetType().Name}: {e.Message}. Disabling system.");
                }
            }
        }
        public void Run()
        {
            Initialize();
            IoCManager.Resolve<EventSystem>().PublishEvent<IApplicationRun>(e => e.OnApplicationRun());
        }
    }
}