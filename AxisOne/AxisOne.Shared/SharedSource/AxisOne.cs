using Arch.Core;
using Arch.Core.Extensions;
using MalignEngine;
using nkast.Aether.Physics2D.Dynamics;
using System.Net;

namespace AxisOne;

public class AxisOne : BaseSystem
{
    [Dependency]
    protected AssetService AssetService = default!;

    [Dependency]
    protected SceneSystem SceneSystem = default!;

    [Dependency]
    protected NetworkingSystem NetworkingSystem = default!;

    [Dependency]
    protected TileSystem TileSystem = default!;

    private EntityRef currentScene;

    private Scene mainmenu;
    private Scene gamescene;

    public override void OnInitialize()
    {
        LoggerService.LogVerbose("Hello from AxisOne!");

        GUIStyle.Default = new GUIStyle()
        {
            NormalFont = AssetService.FromFile<Font>("Content/Roboto-Regular.ttf"),
            FrameTexture = Texture2D.White
        };

        AssetService.LoadFolder("Content");

        {
            mainmenu = new Scene();

            EntityRef camera = mainmenu.Root;
            camera.Add(new Transform());
            camera.Add(new OrthographicCamera() { IsMain = true, ClearColor = Color.Black });

            EntityRef mainMenu = mainmenu.SceneWorld.CreateEntity();
            mainMenu.Add(new MainMenuComponent());
            mainMenu.Add(new ParentOf() { Parent = camera });
        }

        {
            gamescene = new Scene();

            EntityRef camera = gamescene.Root;
            camera.Add(new Transform());
            camera.Add(new OrthographicCamera() { IsMain = true, ClearColor = Color.LightSkyBlue, ViewSize = 10f });

            EntityRef tilemap = TileSystem.CreateTileMap(gamescene.SceneWorld, new TileLayer[] { new TileLayer("Floor", 0, false), new TileLayer("Wall", 1, true) });
            tilemap.Add(new NameComponent("TileMap"));
        }

#if SERVER
        NetworkingSystem.StartServer(7777);
        SceneSystem.Instantiate(gamescene);
#elif CLIENT
        currentScene = SceneSystem.Instantiate(mainmenu);
#endif
    }

    public void LoadGame()
    {
        currentScene.Destroy();
        SceneSystem.Instantiate(gamescene);
    }
}