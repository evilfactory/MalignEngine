using Arch.Core;
using Arch.Core.Extensions;
using MalignEngine;
using System.Net;

namespace AxisOne;

public class AxisOne : BaseSystem
{
    [Dependency]
    protected AssetSystem AssetSystem = default!;

    [Dependency]
    protected SceneSystem SceneSystem = default!;

    [Dependency]
    protected NetworkingSystem NetworkingSystem = default!;

    private EntityRef currentScene;

    private Scene mainmenu;
    private Scene gamescene;

    public override void OnInitialize()
    {
        LoggerService.LogVerbose("Hello from AxisOne!");

        GUIStyle.Default = new GUIStyle()
        {
            NormalFont = AssetSystem.Load<Font>("Content/Roboto-Regular.ttf"),
            FrameTexture = Texture2D.White
        };

        AssetSystem.LoadFolder("Content");

        mainmenu = new Scene((world) =>
        {
            EntityRef camera = world.CreateEntity();
            camera.Add(new Transform());
            camera.Add(new OrthographicCamera() { IsMain = true, ClearColor = Color.Black });

            EntityRef mainMenu = world.CreateEntity();
            mainMenu.Add(new MainMenuComponent());
            mainMenu.Add(new ParentOf() { Parent = camera });

            return camera;
        });

        gamescene = new Scene((world) =>
        {
            EntityRef camera = world.CreateEntity();
            camera.Add(new Transform());
            camera.Add(new OrthographicCamera() { IsMain = true, ClearColor = Color.LightSkyBlue });


            return camera;
        });

#if SERVER
        NetworkingSystem.StartServer(7777);
        LoadGame();
#elif CLIENT
        currentScene = SceneSystem.LoadScene(mainmenu);
#endif
    }

    public void LoadGame()
    {
#if CLIENT
        //NetworkingSystem.Connect(IPEndPoint.Parse("127.0.0.1:7777"));
#endif
        currentScene.Destroy();
        SceneSystem.LoadScene(gamescene);

        EntityRef entity = SceneSystem.LoadScene(AssetSystem.GetOfType<Scene>().Where(x => x.Asset.SceneId == "player").First());
    }
}