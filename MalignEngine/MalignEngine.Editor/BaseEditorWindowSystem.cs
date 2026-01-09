namespace MalignEngine.Editor;

public abstract class BaseEditorWindowSystem : BaseSystem
{
    public abstract string WindowName { get; }
    public bool Active = true;

    protected EditorSystem EditorSystem;
    protected ImGuiSystem ImGuiService;

    protected BaseEditorWindowSystem(IServiceContainer serviceContainer, EditorSystem editorSystem, ImGuiSystem imGuiService) 
        : base(serviceContainer)
    {
        EditorSystem = editorSystem;
        ImGuiService = imGuiService;

        EditorSystem.AddWindow(this);
    }

    public virtual void DrawWindow(float delta) { }

    public override void Dispose()
    {
        base.Dispose();

        EditorSystem.RemoveWindow(this);
    }
}