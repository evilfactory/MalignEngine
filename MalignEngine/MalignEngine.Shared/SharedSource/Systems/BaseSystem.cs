namespace MalignEngine
{
    public abstract class BaseSystem
    {
        public virtual void Initialize() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Draw(float deltaTime) { }
    }
}