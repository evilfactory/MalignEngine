namespace MalignEngine
{
    public abstract class BaseEditorWindowSystem : EntitySystem
    {
        public abstract string WindowName { get; }
        public bool Active;

        [Dependency]
        protected EditorSystem EditorSystem = default!;

        public override void Initialize()
        {
            EditorSystem.AddWindow(this);
        }
    }
}