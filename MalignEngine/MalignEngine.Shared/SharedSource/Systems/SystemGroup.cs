using System.Reflection;

namespace MalignEngine
{
    public class SystemGroup
    {
        private readonly List<BaseSystem> systems;

        public SystemGroup()
        {
            systems = new List<BaseSystem>();
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
                }
                catch (Exception e)
                {
                    system.Enabled = false;
                    Logger.LogError($"Failed to inject dependencies to system {system.GetType().Name}: {e.Message}. Disabling system.");
                }
            }

            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.Initialize();
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.BeforeUpdate(deltaTime);
            }
            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.Update(deltaTime);
            }
            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.AfterUpdate(deltaTime);
            }
        }

        public void Draw(float deltaTime)
        {
            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.BeforeDraw(deltaTime);
            }
            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.Draw(deltaTime);
            }
            foreach (var system in systems)
            {
                if (!system.Enabled) { continue; }
                system.AfterDraw(deltaTime);
            }
        }
    }
}