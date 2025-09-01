using ImGuiNET;
using Silk.NET.Maths;
using System.Numerics;
using System.Xml.Linq;

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
    [Dependency]
    protected SceneXmlLoader SceneXmlLoader = default!;
    [Dependency]
    protected ParentSystem ParentSystem = default!;

    public override string WindowName => "Tile Editor";

    private EntityRef _selectedTileMap;
    private TileData _selectedTileData;

    private AssetHandle<TileList>[] _tileList;

    private string _fileName = "Content/tilemap.xml";

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
        ImGui.Begin(WindowName, ImGuiWindowFlags.NoScrollbar);

        ImGui.Text("Tile Editor");

        List<EntityRef> tilemaps = new List<EntityRef>();

        EntityManager.World.Query(EntityManager.World.CreateQuery().WithAll<TileMapComponent>(), (EntityRef entity) =>
        {
            tilemaps.Add(entity);
        });

        if (ImGui.BeginCombo("Tilemaps", _selectedTileMap.IsValid() ? GetTileName(_selectedTileMap) : "Select Tilemap"))
        {
            foreach (EntityRef tilemap in tilemaps)
            {
                if (ImGui.Selectable(GetTileName(tilemap), _selectedTileMap == tilemap))
                {
                    _selectedTileMap = tilemap;
                }
            }

            ImGui.EndCombo();
        }

        if (ImGui.Button("Save"))
        {
            XElement element = new XElement("Scene");

            var children = _selectedTileMap.Get<Children>();

            EntityRef[] entitiesToCopy = new EntityRef[children.Childs.Count + 1];

            entitiesToCopy[0] = _selectedTileMap;

            int index = 1;
            foreach (EntityRef entity in children.Childs)
            {
                entitiesToCopy[index] = entity;
                index++;
            }

            var scene = new Scene("savedmap", entitiesToCopy);
            SceneXmlLoader.Save(element, scene);
            element.Save(_fileName);
        }

        ImGui.SameLine();

        if (ImGui.Button("Load"))
        {
            Scene scene = AssetService.FromPath<Scene>(_fileName);
            EntityRef tilemap = SceneSystem.Instantiate(scene);
            _selectedTileMap = tilemap;
        }

        ImGui.SameLine();

        ImGui.InputText("File Name", ref _fileName, 100);


        if (_tileList.Length > 0)
        {
            TileList selectedTileList = _tileList[0];

            // make a grid of all tiles

            int gridMax = Math.Max((int)MathF.Floor((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X) / 100f), 1) - 1;
            ImGui.Columns(gridMax, null, false);

            foreach (TileData tile in selectedTileList.Tiles)
            {
                if (ImGuiService.ImageButton(tile.Scene.SceneId, tile.Icon.Asset.Texture.Resource, new Vector2(100, 100), tile.Icon.Asset.UV1, tile.Icon.Asset.UV2))
                {
                    _selectedTileData = tile;
                }
                ImGui.Text(tile.Scene.SceneId);

                ImGui.NextColumn();
            }
        }

        ImGui.End();
    }

    private Vector2D<int> lastPlacePosition = new Vector2D<int>();
    private TileData lastPlacedTile = null;

    public override void OnUpdate(float deltaTime)
    {
        _tileList = AssetService.GetHandles<TileList>().ToArray();

        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!_selectedTileMap.IsValid() || _selectedTileData == null) { return; }

        if (InputService.Mouse.IsButtonPressed(MouseButton.Left))
        {
            Vector2 mousePosition = EditorSceneViewSystem.WorldMousePosition;
            Vector2D<int> tilePosition = new Vector2D<int>((int)MathF.Round(mousePosition.X), (int)MathF.Round(mousePosition.Y));

            if (lastPlacePosition != tilePosition || lastPlacedTile != _selectedTileData)
            {
                EntityRef tile = TileSystem.CreateTile(_selectedTileMap, _selectedTileData, tilePosition);
                lastPlacePosition = tilePosition;
                lastPlacedTile = _selectedTileData;
            }
        }
    }

    public void OnCameraDraw(float delta, OrthographicCamera camera)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (!_selectedTileMap.IsValid() || _selectedTileData == null) { return; }

        RenderingAPI.Submit(ctx =>
        {
            Renderer2D.Begin(ctx);
            SpriteRenderingSystem.DrawSprite(_selectedTileData.Icon, new Vector2(MathF.Round(EditorSceneViewSystem.WorldMousePosition.X), MathF.Round(EditorSceneViewSystem.WorldMousePosition.Y)), Vector2.One, Color.White);
            Renderer2D.End();
        });
    }
}
