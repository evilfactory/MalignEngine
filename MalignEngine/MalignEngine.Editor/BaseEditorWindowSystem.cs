namespace MalignEngine.Editor;

public abstract class BaseEditorWindowSystem : BaseSystem
{
    public abstract string WindowName { get; }
    public bool Active = true;

    protected EditorSystem EditorSystem;
    protected ImGuiSystem ImGuiService;

    protected BaseEditorWindowSystem(ILoggerService loggerService, IScheduleManager scheduleManager, EditorSystem editorSystem, ImGuiSystem imGuiService) 
        : base(loggerService, scheduleManager)
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