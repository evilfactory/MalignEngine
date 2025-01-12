using MalignEngine;

namespace SnakeGame;

class SnakeGame : EntitySystem
{
    [Dependency]
    protected AssetService AssetService = default!;

#if CLIENT
    [Dependency]
    protected MainMenu MainMenu = default!;
#elif SERVER
    [Dependency]
    protected SnakeServer SnakeServer = default!;
#endif

    public override void OnInitialize()
    {
        LoggerService.LogInfo("SnakeGame initialized");

        AssetService.LoadFolder("Content");

#if SERVER
        SnakeServer.Listen(8080);

#elif CLIENT
        GUIStyle.Default = new GUIStyle()
        {
            NormalFont = AssetService.FromFile<Font>("Content/Roboto-Regular.ttf"),
            FrameTexture = Texture2D.White
        };

        MainMenu.ShowMainMenu = true;
#endif
    }
}