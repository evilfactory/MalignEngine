namespace MalignEngine.Sample;

public class SampleInit : IService, IInit
{
    [Dependency]
    protected AssetService AssetService = default!;
    [Dependency]
    protected StateManager StateManager = default!;

    public void OnInitialize()
    {
        GUIStyle.Default = new GUIStyle()
        {
            FrameTexture = Texture2D.White,
            NormalFont = AssetService.FromFile<Font>("Content/Roboto-Regular.ttf")
        };

        StateManager.Next(GameState.MainMenu);
    }
}