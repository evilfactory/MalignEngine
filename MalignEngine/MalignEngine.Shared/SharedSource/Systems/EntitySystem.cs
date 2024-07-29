using Arch.Core;

namespace MalignEngine
{
    public abstract class EntitySystem : BaseSystem
    {
        [Dependency]
        protected World World = default!;
        [Dependency]
        protected EventBus EventBus = default!;
    }
}