using Arch.Core;

namespace MalignEngine
{
    public interface ISystem : IInit, IUpdate, IDraw
    {
    }

    public abstract class BaseSystem : ISystem
    {
        public bool Enabled { get; set; } = true;

        public virtual void OnInitialize() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnDraw(float deltaTime) { }
    }
}