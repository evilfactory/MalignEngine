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
    protected SpriteRenderingSystem SpriteRenderingSystem = default!;
    [Dependency]
    protected TileSystem TileSystem = default!;
    [Dependency]
    protected InputSystem InputSystem = default!;

    public override string WindowName => "Tile Editor";

    private EntityRef selectedTileMap;
    private TileData selectedTileData;

    private string GetTileName(EntityRef tilemap)
    {
        if (tilemap.TryGet<NameComponent>(out var nameComp))
        {
            return nameComp.Name;
        }

        return tilemap.Id.ToString();
    }

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

        if (ImGui.BeginCombo("Tilemaps", selectedTileMap.IsValid() ? GetTileName(selectedTileMap) : "Select Tilemap"))
        {
            foreach (EntityRef tilemap in tilemaps)
            {
                if (ImGui.Selectable(GetTileName(tilemap), selectedTileMap == tilemap))
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
            if (ImGuiSystem.ImageButton(tile.SceneId, tile.Icon.Texture, new Vector2(100, 100), tile.Icon.UV1, tile.Icon.UV2))
            {
                selectedTileData = tile;
            }
            ImGui.Text(tile.SceneId);

            ImGui.NextColumn();
        }

        ImGui.End();
    }

    private Vector2D<int> lastPlacePosition = new Vector2D<int>();
    private TileData lastPlacedTile = null;

    public override void OnUpdate(float deltaTime)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!selectedTileMap.IsValid() || selectedTileData == null) { return; }

        if (InputSystem.IsMouseButtonPressed(0))
        {
            Vector2 mousePosition = EditorSceneViewSystem.WorldMousePosition;
            int xTilePosition = (int)MathF.Round(mousePosition.X);
            int yTilePosition = (int)MathF.Round(mousePosition.Y);

            if (lastPlacePosition != new Vector2D<int>(xTilePosition, yTilePosition) || lastPlacedTile != selectedTileData)
            {
                TileSystem.SetTile(selectedTileMap, AssetSystem.GetOfType<Scene>().Where(x => x.Asset.SceneId == selectedTileData.SceneId).First(), xTilePosition, yTilePosition);
                lastPlacePosition = new Vector2D<int>(xTilePosition, yTilePosition);
                lastPlacedTile = selectedTileData;
            }
        }
    }

    public override void OnDraw(float deltaTime)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!selectedTileMap.IsValid() || selectedTileData == null) { return; }

        RenderingSystem.Begin();
        SpriteRenderingSystem.DrawSprite(selectedTileData.Icon, new Vector2(MathF.Round(EditorSceneViewSystem.WorldMousePosition.X), MathF.Round(EditorSceneViewSystem.WorldMousePosition.Y)), Vector2.One, Color.White);
        RenderingSystem.End();
    }
}