namespace MalignEngine.Editor;

public abstract class BaseEditorWindowSystem : BaseSystem
{
    public abstract string WindowName { get; }
    public bool Active = true;

    protected EditorSystem EditorSystem;
    protected ImGuiService ImGuiService;

    public BaseEditorWindowSystem(EditorSystem editorSystem, ImGuiService imGuiService)
    {
        EditorSystem = editorSystem;
        ImGuiService = imGuiService;

        EditorSystem.AddWindow(this);
    }

    public virtual void DrawWindow(float delta) { }
}