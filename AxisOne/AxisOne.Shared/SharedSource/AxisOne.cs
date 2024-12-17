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
    protected LoggerService LoggerService = default!;

    [Dependency]
    protected SceneSystem SceneSystem = default!;

    [Dependency]
    protected NetworkingSystem NetworkingSystem = default!;

    public override void OnInitialize()
    {
        LoggerService.LogVerbose("Hello from AxisOne!");

        GUIStyle.Default = new GUIStyle()
        {
            NormalFont = AssetSystem.Load<Font>("Content/Roboto-Regular.ttf"),
            FrameTexture = Texture2D.White
        };

        Scene mainmenu = AssetSystem.Add(new Scene((world) =>
        {
            EntityRef camera = world.CreateEntity();
            camera.Add(new Transform());
            camera.Add(new OrthographicCamera() { IsMain = true, ClearColor = Color.Black });

            EntityRef mainMenu = world.CreateEntity();
            mainMenu.Add(new MainMenuComponent());
            mainMenu.Add(new ParentOf() { Parent = camera });

            return camera;
        }));

#if SERVER
        NetworkingSystem.StartServer(7777);
        LoadGame();
#elif CLIENT
        SceneSystem.LoadScene(mainmenu);
#endif

        Scene scene = AssetSystem.Load<Scene>("Content/Scenes/Player.xml");
        EntityRef entity = SceneSystem.LoadScene(scene);

        Scene sceneSaved = SceneSystem.SaveScene(entity);
        sceneSaved.Save("Content/Scenes/PlayerSaved.xml");
    }

    public void LoadGame()
    {
#if CLIENT
        NetworkingSystem.Connect(IPEndPoint.Parse("127.0.0.1:7777"));
#endif
        // SceneSystem.LoadScene(gameScene);
    }
}