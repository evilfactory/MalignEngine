using MalignEngine;
using MalignEngine.Editor;
using ImGuiNET;
using Silk.NET.Maths;
using System.Numerics;
using System.Xml.Linq;

namespace SteamBolt;

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
    protected HierarchySystem ParentSystem = default!;

    public override string WindowName => "Tile Editor";

    private Entity _selectedTileMapEntity;
    private TileMapComponent? _selectedTileMap = null;
    private TileDefinition? _selectedTileDefinition;

    private AssetHandle<TileList>[]? _tileList;

    private string _fileName = "Content/tilemap.xml";

    public EditorTile(IServiceContainer serviceContainer, ILoggerService loggerService, IScheduleManager scheduleManager, EditorSystem editorSystem, ImGuiSystem imGuiService)
        : base(serviceContainer, editorSystem, imGuiService)
    {
        scheduleManager.Register<ICameraDraw>(this);
    }

    public override void DrawWindow(float delta)
    {
        ImGui.Begin(WindowName, ImGuiWindowFlags.NoScrollbar);

        ImGui.Text("Tile Editor");

        List<Entity> tilemaps = new List<Entity>();

        EntityManager.World.Query(new Query().WithAll<TileMapComponent>(), tilemaps.Add);

        if (ImGui.BeginCombo("Tilemaps", _selectedTileMap == null ? "Selected" : "Select Tilemap"))
        {
            foreach (Entity tilemap in tilemaps)
            {
                if (ImGui.Selectable(tilemap.Id.ToString(), _selectedTileMap == tilemap.Get<TileMapComponent>()))
                {
                    _selectedTileMap = tilemap.Get<TileMapComponent>();
                    _selectedTileMapEntity = tilemap;
                }
            }

            ImGui.EndCombo();
        }

        if (ImGui.Button("Save"))
        {

        }

        ImGui.SameLine();

        if (ImGui.Button("Load"))
        {
            //Scene scene = AssetService.FromPath<Scene>(_fileName);
            //Entity tilemap = SceneSystem.Instantiate(scene);
            //_selectedTileMap = tilemap;
        }

        ImGui.SameLine();

        ImGui.InputText("File Name", ref _fileName, 100);


        if (_tileList.Length > 0)
        {
            TileList selectedTileList = _tileList[0];

            // make a grid of all tiles

            ImGui.Columns(selectedTileList.Definitions.Count, null, false);

            foreach (TileDefinition tile in selectedTileList.Definitions)
            {
                if (ImGuiService.ImageButton(tile.Identifier, tile.Sprite.Asset.Texture.Resource, new Vector2(100, 100), tile.Sprite.Asset.UV1, tile.Sprite.Asset.UV2))
                {
                    _selectedTileDefinition = tile;
                }
                ImGui.Text(tile.Identifier);

                ImGui.NextColumn();
            }
        }

        ImGui.End();
    }

    private Vector2D<int> lastPlacePosition = new Vector2D<int>();
    private TileDefinition? lastPlacedTile = null;

    private Vector2D<int> GetMouseTilePosition(Entity tilemap)
    {
        Vector2 mousePosition = EditorSceneViewSystem.WorldMousePosition;
        if (tilemap.TryGet(out ComponentRef<Transform> transform))
        {
            mousePosition -= transform.Value.Position.ToVector2();
        }
        return new Vector2D<int>((int)MathF.Round(mousePosition.X), (int)MathF.Round(mousePosition.Y));
    }

    public override void OnUpdate(float deltaTime)
    {
        _tileList = AssetService.GetHandles<TileList>().ToArray();

        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (_selectedTileMap == null || _selectedTileDefinition == null) { return; }

        if (InputService.Mouse.IsButtonPressed(MouseButton.Left))
        {
            Vector2D<int> tilePosition = GetMouseTilePosition(_selectedTileMapEntity);

            if (lastPlacePosition != tilePosition || lastPlacedTile != _selectedTileDefinition)
            {
                TileSystem.SetTile(_selectedTileMap, _selectedTileDefinition.LayerId, tilePosition, _selectedTileDefinition);
                lastPlacePosition = tilePosition;
                lastPlacedTile = _selectedTileDefinition;
            }
        }
    }

    public void OnCameraDraw(float delta, OrthographicCamera camera)
    {
        if (!EditorSceneViewSystem.IsWindowHovered) { return; }
        if (_selectedTileMap == null || _selectedTileDefinition == null) { return; }

        RenderingAPI.Submit(ctx =>
        {
            Vector2 mousePosition = EditorSceneViewSystem.WorldMousePosition;

            Renderer2D.Begin(ctx);
            SpriteRenderingSystem.DrawSprite(_selectedTileDefinition.Sprite, new Vector2((int)MathF.Round(mousePosition.X), (int)MathF.Round(mousePosition.Y)), Vector2.One, Color.White);
            Renderer2D.End();
        });
    }
}