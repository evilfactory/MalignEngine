using Arch.Core;
using ImGuiNET;
using System.Numerics;

namespace MalignEngine
{
    public class EditorSystem : BaseSystem
    {
        public EntityReference SelectedEntity { get; set; } = EntityReference.Null;

        private readonly List<BaseEditorWindowSystem> windows = new List<BaseEditorWindowSystem>();

        public void AddWindow(BaseEditorWindowSystem window)
        {
            windows.Add(window);
        }

        public override void OnDraw(float deltaTime)
        {
            uint dockspaceId = ImGui.GetID("MyDockSpace");
            ImGui.DockSpace(dockspaceId, new Vector2(0, 0), ImGuiDockNodeFlags.PassthruCentralNode);


            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Windows"))
                {
                    for (int i = 0; i < windows.Count; i++)
                    {
                        ImGui.MenuItem(windows[i].WindowName, null, ref windows[i].Active);
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }
    }
}