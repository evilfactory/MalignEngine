using ImGuiNET;
using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine;

/*
public class EditorTile : BaseEditorWindowSystem
{
    [Dependency]
    protected AssetService AssetService = default!;
    [Dependency]
    protected EditorSceneViewSystem EditorSceneViewSystem = default!;
    [Dependency]
    protected IRenderer2D IRenderingService = default!;
    [Dependency]
    protected SpriteRenderingSystem SpriteRenderingSystem = default!;
    [Dependency]
    protected TileSystem TileSystem = default!;
    [Dependency]
    protected InputSystem InputSystem = default!;
    [Dependency]
    protected SceneSystem SceneSystem = default!;

    public override string WindowName => "Tile Editor";

    private EntityRef selectedTileMap;
    private TileData selectedTileData;

    private string fileName = "Content/tilemap.xml";

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
        AssetHandle<TileList>[] tilelists = AssetService.GetOfType<TileList>().ToArray();

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

        if (ImGui.Button("Save"))
        {
            //Scene scene = SceneSystem.SaveScene(selectedTileMap);
            //scene.Save(fileName);
        }

        ImGui.SameLine();

        if (ImGui.Button("Load"))
        {
            //Scene scene = Scene.Load(fileName);
            //EntityRef tilemap = SceneSystem.LoadScene(scene);
            //selectedTileMap = tilemap;
        }

        ImGui.SameLine();

        ImGui.InputText("File Name", ref fileName, 100);

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
                TileSystem.SetTile(selectedTileMap, AssetService.GetFromId<Scene>(selectedTileData.SceneId), xTilePosition, yTilePosition);
                lastPlacePosition = new Vector2D<int>(xTilePosition, yTilePosition);
                lastPlacedTile = selectedTileData;
            }
        }
    }

    public override void OnDraw(float deltaTime)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!selectedTileMap.IsValid() || selectedTileData == null) { return; }

        IRenderingService.Begin();
        SpriteRenderingSystem.DrawSprite(selectedTileData.Icon, new Vector2(MathF.Round(EditorSceneViewSystem.WorldMousePosition.X), MathF.Round(EditorSceneViewSystem.WorldMousePosition.Y)), Vector2.One, Color.White);
        IRenderingService.End();
    }
}
*/