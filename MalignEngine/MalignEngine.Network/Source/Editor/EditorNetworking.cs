using ImGuiNET;
using System.Numerics;

namespace MalignEngine;

public class EditorNetworking : BaseEditorWindowSystem
{
    [Dependency]
    protected NetworkingSystem NetworkingSystem = default!;

    public override string WindowName => "Network Viewer";

    private NetworkConnection selectedConnection;

    public override void DrawWindow(float delta)
    {
        ImGui.Begin(WindowName, ImGuiWindowFlags.NoScrollbar);

#if SERVER
        if (ImGui.BeginTable("split", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.Resizable))
        {
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                ImGui.BeginChild("scrolling", new Vector2(0, 0), false);

                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.1f, 1f), "Connections");

                foreach (NetworkConnection connection in NetworkingSystem.Connections)
                {
                    if (ImGui.Selectable(connection.ToString(), selectedConnection == connection))
                    {
                        selectedConnection = connection;
                    }
                }

                ImGui.EndChild();
            }
            {
                ImGui.TableSetColumnIndex(1);

                ImGui.BeginChild("scrolling2", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);

                if (selectedConnection != null)
                {
                    ImGui.Text($"Id: {selectedConnection.Id}");
                }

                ImGui.EndChild();
            }

            ImGui.EndTable();
        }

#elif CLIENT
        if (NetworkingSystem.Connection != null)
        {
            ImGui.Text($"Connected, client id = {NetworkingSystem.Connection.Id}");
        }
        else
        {
            ImGui.Text("Not connected");
        }
#endif

        ImGui.End();
    }
}