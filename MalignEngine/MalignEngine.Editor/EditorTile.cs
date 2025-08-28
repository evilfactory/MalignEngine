using ImGuiNET;
using Silk.NET.Maths;
using System.Numerics;

namespace MalignEngine.Editor;

public class EditorTile : BaseEditorWindowSystem, ICameraDraw
{
    [Dependency]
    protected IAssetService AssetService = default!;
    [Dependency]
    protected EditorSceneViewSystem EditorSceneViewSystem = default!;
    [Dependency]
    protected IRenderingAPI RenderingAPI = default!;
    [Dependency]
    protected IRenderer2D Renderer2D = default!;
    [Dependency]
    protected SpriteRenderingSystem SpriteRenderingSystem = default!;
    [Dependency]
    protected TileSystem TileSystem = default!;
    [Dependency]
    protected IInputService InputService = default!;
    [Dependency]
    protected SceneSystem SceneSystem = default!;
    [Dependency]
    protected IEntityManager EntityManager = default!;

    public override string WindowName => "Tile Editor";

    private EntityRef selectedTileMap;
    private TileData selectedTileData;

    private string fileName = "Content/tilemap.xml";

    public EditorTile(EditorSystem editorSystem, ImGuiService imGuiService) : base(editorSystem, imGuiService)
    {

    }

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
        AssetHandle<TileList>[] tilelists = AssetService.GetHandles<TileList>().ToArray();

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
            if (ImGuiService.ImageButton(tile.Scene.SceneId, tile.Icon.Asset.Texture.Resource, new Vector2(100, 100), tile.Icon.UV1, tile.Icon.UV2))
            {
                selectedTileData = tile;
            }
            ImGui.Text(tile.Scene.SceneId);

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

        if (InputService.Mouse.IsButtonPressed(MouseButton.Left))
        {
            Vector2 mousePosition = EditorSceneViewSystem.WorldMousePosition;
            Vector2D<int> tilePosition = new Vector2D<int>((int)MathF.Round(mousePosition.X), (int)MathF.Round(mousePosition.Y));

            if (lastPlacePosition != tilePosition || lastPlacedTile != selectedTileData)
            {
                EntityRef tile = TileSystem.CreateTile(selectedTileMap, selectedTileData.LayerId, tilePosition);
                lastPlacePosition = tilePosition;
                lastPlacedTile = selectedTileData;
            }
        }
    }

    public void OnCameraDraw(float delta, OrthographicCamera camera)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!selectedTileMap.IsValid() || selectedTileData == null) { return; }

        RenderingAPI.Submit(ctx =>
        {
            Renderer2D.Begin(ctx);
            SpriteRenderingSystem.DrawSprite(selectedTileData.Icon, new Vector2(MathF.Round(EditorSceneViewSystem.WorldMousePosition.X), MathF.Round(EditorSceneViewSystem.WorldMousePosition.Y)), Vector2.One, Color.White);
            Renderer2D.End();
        });
    }
}
