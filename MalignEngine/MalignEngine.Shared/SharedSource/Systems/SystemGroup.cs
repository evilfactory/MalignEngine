using System.Reflection;

namespace MalignEngine
{
    public class SystemGroup
    {
        private readonly List<BaseSystem> systems;

        public SystemGroup(List<BaseSystem> add)
        {
            systems = new List<BaseSystem>();

            foreach (var system in add)
            {
                AddSystem(system);
            }
        }
        public void AddSystem(BaseSystem system)
        {
            systems.Add(system);
            IoCManager.Register(system);
        }

        public void Initialize()
        {
            foreach (var system in systems)
            {
                IoCManager.InjectDependencies(system);
            }

            foreach (var system in systems)
            {
                system.Initialize();
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var system in systems)
            {
                system.Update(deltaTime);
            }
        }

        public void Draw(float deltaTime)
        {
            foreach (var system in systems)
            {
                system.Draw(deltaTime);
            }
        }

    }
}