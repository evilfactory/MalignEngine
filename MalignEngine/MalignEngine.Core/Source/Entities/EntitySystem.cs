using Arch.Core;
using Arch.Core.Extensions;
using System.ComponentModel.Design;
using System.Reflection;

namespace MalignEngine
{
    public abstract class EntitySystem : BaseSystem
    {
        [Dependency]
        protected EntityManagerService EntityManager = default!;
        [Dependency]
        protected EventService EventService = default!;

        public static bool Resolve<T>(in EntityRef entity, ref T comp) where T : IComponent
        {
            bool success = false;
            comp = entity.TryGetRef<T>(out success);
            return success;
        }
    }
}