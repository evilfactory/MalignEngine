using ImGuiNET;
using System.Numerics;

namespace MalignEngine.Editor;

public class EditorSystem : BaseSystem, IDrawImGui
{
    [Dependency]
    protected IInputService InputSystem = default!;

    public Entity SelectedEntity { get; set; } = default;

    private bool hideAllWindows = true;

    private readonly List<BaseEditorWindowSystem> windows = new List<BaseEditorWindowSystem>();

    public EditorSystem(IServiceContainer serviceContainer) : base(serviceContainer) { }

    public void AddWindow(BaseEditorWindowSystem window)
    {
        windows.Add(window);
    }

    public void RemoveWindow(BaseEditorWindowSystem window)
    {
        windows.Remove(window);
    }

    public override void OnUpdate(float deltaTime)
    {
        if (InputSystem.Keyboard.WasKeyPressed(Key.F1))
        {
            hideAllWindows = !hideAllWindows;
        }
    }

    public void OnDrawImGui(float deltaTime)
    {
        uint dockspaceId = ImGui.GetID("MyDockSpace");
        ImGui.DockSpace(dockspaceId, new Vector2(0, 0), ImGuiDockNodeFlags.PassthruCentralNode);


        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Windows"))
            {
                ImGui.MenuItem("Hide all", null, ref hideAllWindows);

                for (int i = 0; i < windows.Count; i++)
                {
                    ImGui.MenuItem(windows[i].WindowName, null, ref windows[i].Active);
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        if (!hideAllWindows)
        {
            foreach (BaseEditorWindowSystem window in windows)
            {
                if (!window.Active) { continue; }

                window.DrawWindow(deltaTime);
            }
        }
    }
}