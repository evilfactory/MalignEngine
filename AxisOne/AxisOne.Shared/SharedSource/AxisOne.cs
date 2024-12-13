using Arch.Core;
using Arch.Core.Extensions;
using MalignEngine;

namespace AxisOne;

public class AxisOne : BaseSystem
{
    [Dependency]
    protected AssetSystem AssetSystem = default!;

    [Dependency]
    protected LoggerService LoggerService = default!;

    [Dependency]
    protected SceneSystem SceneSystem = default!;

    private Scene mainMenuScene;

    public override void OnInitialize()
    {
        LoggerService.LogVerbose("Hello from AxisOne!");

        GUIStyle.Default = new GUIStyle()
        {
            NormalFont = AssetSystem.Load<Font>("Content/Roboto-Regular.ttf"),
            FrameTexture = Texture2D.White
        };

        mainMenuScene = new Scene((create, world) =>
        {
            EntityRef camera = create();
            camera.Add(new Transform());
            camera.Add(new OrthographicCamera() { IsMain = true, ClearColor = Color.Black});

            EntityRef mainMenu = create();
            mainMenu.Add(new MainMenuComponent());
        });

        SceneSystem.LoadScene(mainMenuScene);
    }
}