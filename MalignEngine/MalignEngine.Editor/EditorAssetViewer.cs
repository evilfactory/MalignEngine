using ImGuiNET;
using System.Numerics;

namespace MalignEngine.Editor;

public class EditorAssetViewer : BaseEditorWindowSystem
{
    private IAssetService _assetService;
    private object? selectedAsset;

    public override string WindowName => "Asset Viewer";

    public EditorAssetViewer(EditorSystem editorSystem, ImGuiService imGuiService, IAssetService assetService) : base(editorSystem, imGuiService)
    {
        _assetService = assetService;
    }

    public override void DrawWindow(float delta)
    {
        ImGui.Begin(WindowName, ImGuiWindowFlags.NoScrollbar);

        if (ImGui.BeginTable("split", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.Resizable))
        {
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();

                ImGui.BeginChild("scrolling", new Vector2(0, 0), false);

                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.1f, 1f), "Texture2D");

                foreach (AssetHandle<Texture2D> handle in _assetService.GetHandles<Texture2D>())
                {
                    if (ImGui.Selectable(handle.AssetPath, selectedAsset == handle))
                    {
                        selectedAsset = handle;
                    }
                }

                ImGui.EndChild();
            }
            {
                ImGui.TableSetColumnIndex(1);

                ImGui.BeginChild("scrolling2", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);

                if (selectedAsset is AssetHandle<Texture2D> texture2d)
                {
                    ImGui.Text($"Path: {texture2d.AssetPath}");

                    Texture2D asset = texture2d.Asset;

                    if (asset == null)
                    {
                        ImGui.Text("Asset not loaded");
                    }
                    else
                    {
                        ImGui.Text($"Size: {asset.Width}x{asset.Height}");
                        ImGuiService.Image(asset.Resource, new Vector2(asset.Width, asset.Height));
                    }
                }

                ImGui.EndChild();
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}