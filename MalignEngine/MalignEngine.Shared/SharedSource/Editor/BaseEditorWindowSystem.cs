namespace MalignEngine
{
    public abstract class BaseEditorWindowSystem : EntitySystem
    {
        public abstract string WindowName { get; }
        public bool Active;

        [Dependency]
        protected EditorSystem EditorSystem = default!;

        [Dependency]
        protected ImGuiSystem ImGuiSystem = default!;

        public override void OnInitialize()
        {
            EditorSystem.AddWindow(this);
        }
    }
}