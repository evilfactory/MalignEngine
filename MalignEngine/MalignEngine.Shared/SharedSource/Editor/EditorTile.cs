using ImGuiNET;
using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;

public class EditorTile : BaseEditorWindowSystem
{
    [Dependency]
    protected AssetSystem AssetSystem = default!;
    [Dependency]
    protected EditorSceneViewSystem EditorSceneViewSystem = default!;
    [Dependency]
    protected RenderingSystem RenderingSystem = default!;
    [Dependency]
    protected TileSystem TileSystem = default!;
    [Dependency]
    protected InputSystem InputSystem = default!;

    public override string WindowName => "Tile Editor";

    private EntityRef selectedTileMap;
    private TileData selectedTileData;

    public override void DrawWindow(float delta)
    {
        AssetHandle<TileList>[] tilelists = AssetSystem.GetOfType<TileList>().ToArray();

        ImGui.Begin(WindowName, ImGuiWindowFlags.NoScrollbar);

        ImGui.Text("Tile Editor");

        List<EntityRef> tilemaps = new List<EntityRef>();

        EntityManager.World.Query(EntityManager.World.CreateQuery().WithAll<TileMapComponent>(), (EntityRef entity) =>
        {
            tilemaps.Add(entity);
        });

        if (ImGui.BeginCombo("Tilemaps", "Select Tilemap"))
        {
            foreach (EntityRef tilemap in tilemaps)
            {
                string name = tilemap.Id.ToString();
                if (tilemap.TryGet<NameComponent>(out var nameComp))
                {
                    name = nameComp.Name;
                }

                if (ImGui.Selectable(name, selectedTileMap == tilemap))
                {
                    selectedTileMap = tilemap;
                }
            }

            ImGui.EndCombo();
        }

        TileList selectedTileList = tilelists[0];

        // make a grid of all tiles

        int gridMax = Math.Max((int)MathF.Floor((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X) / 100f), 1) - 1;
        ImGui.Columns(gridMax, null, false);

        foreach (TileData tile in selectedTileList.Tiles)
        {
            if (ImGuiSystem.ImageButton("", tile.Icon, new Vector2(100, 100)))
            {
                selectedTileData = tile;
            }
            ImGui.Text(tile.SceneId);

            ImGui.NextColumn();
        }

        ImGui.End();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!selectedTileMap.IsValid() || selectedTileData == null) { return; }

        if (InputSystem.IsMouseButtonPressed(0))
        {
            Vector2 mousePosition = EditorSceneViewSystem.WorldMousePosition;
            int xTilePosition = (int)MathF.Round(mousePosition.X);
            int yTilePosition = (int)MathF.Round(mousePosition.Y);

            TileSystem.SetTile(selectedTileMap, AssetSystem.GetOfType<Scene>().Where(x => x.Asset.SceneId == selectedTileData.SceneId).First(), xTilePosition, yTilePosition);
        }
    }

    public override void OnDraw(float deltaTime)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!selectedTileMap.IsValid() || selectedTileData == null) { return; }

        RenderingSystem.Begin();
        RenderingSystem.DrawTexture2D(selectedTileData.Icon, new Vector2(MathF.Round(EditorSceneViewSystem.WorldMousePosition.X), MathF.Round(EditorSceneViewSystem.WorldMousePosition.Y)), new Vector2(1f, 1f), Color.White, 0f, 15);
        RenderingSystem.End();
    }
}