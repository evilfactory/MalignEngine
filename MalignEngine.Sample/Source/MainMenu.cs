using MalignEngine;
using System.Net;
using System.Numerics;

namespace MalignEngine.Sample;

public class MainMenu : EntitySystem, IAddToUpdateGUIList
{
    [Dependency]
    protected WindowService WindowSystem = default!;
    [Dependency]
    protected IRenderer2D RenderingService = default!;
    [Dependency]
    protected StateManager StateManager = default!;

    private GUIFrame mainFrame;

    public override void OnInitialize()
    {
        mainFrame = new GUIFrame(new RectTransform(null, new Vector2(1f, 1f), Anchor.Center, Pivot.Center), Color.Transparent);
        var list = new GUIList(new RectTransform(mainFrame.RectTransform, new Vector2(0.7f, 0.7f), Anchor.Center, Pivot.Center), 25);
        new GUIText(new RectTransform(list.RectTransform, new Vector2(1f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), "Select a Test", 100, Color.White);

        string[] names = Enum.GetNames<GameState>();
        GameState[] values = Enum.GetValues<GameState>();

        for (int i = 1; i < names.Length; i++)
        {
            GameState state = values[i];
            var button = new GUIButton(new RectTransform(list.RectTransform, new Vector2(0.8f, 0.07f), Anchor.TopCenter, Pivot.TopCenter), () =>
            {
                StateManager.Next(state);
            });
            button.RectTransform.MinSize = new Vector2(400, 50);
            new GUIText(new RectTransform(button.RectTransform, new Vector2(1f, 1f), Anchor.Center, Pivot.Center), names[i], 50, Color.White);

        }
    }

    public void AddToUpdateList(GUIService gui)
    {
        gui.AddToUpdateList(mainFrame);
    }
}
