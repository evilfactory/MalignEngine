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
        protected EntityEventSystem EntityEventSystem = default!;

        public static bool Resolve<T>(in Entity entity, ref T comp)
        {
            bool success = false;
            comp = entity.TryGetRef<T>(out success);
            return success;
        }
    }
}