using Arch.Core;

namespace MalignEngine
{
    public abstract class BaseSystem
    {
        public SystemGroup SystemGroup { get; set; }
        public bool Enabled { get; set; } = true;

        public virtual void Initialize() { }
        public virtual void BeforeUpdate(float deltaTime) { }
        public virtual void Update(float deltaTime) { }
        public virtual void AfterUpdate(float deltaTime) { }
        public virtual void BeforeDraw(float deltaTime) { }
        public virtual void Draw(float deltaTime) { }
        public virtual void AfterDraw(float deltaTime) { }
        public virtual void DrawGUI(float deltaTime) { }

    }
}