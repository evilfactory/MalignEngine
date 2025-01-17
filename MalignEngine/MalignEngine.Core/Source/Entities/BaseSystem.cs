using Arch.Core;

namespace MalignEngine
{
    public interface IService { }

    public abstract class BaseSystem : IService, IInit, IUpdate, IDraw
    {
        [Dependency]
        protected LoggerService LoggerService = default!;

        public virtual void OnInitialize() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnDraw(float deltaTime) { }
    }
}