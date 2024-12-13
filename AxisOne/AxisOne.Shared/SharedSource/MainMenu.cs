using MalignEngine;
using System.Numerics;

namespace AxisOne;

public struct MainMenuComponent : IComponent { }

public class MainMenu : EntitySystem, IDrawGUI
{
    [Dependency]
    protected WindowSystem WindowSystem = default!;
    [Dependency]
    protected RenderingSystem RenderingSystem = default!;

    private GUIFrame mainFrame;

    public override void OnInitialize()
    {
        mainFrame = new GUIFrame(new RectTransform(null, new Vector2(1f, 1f), Anchor.Center, Pivot.Center), Color.Transparent);
        var list = new GUIList(new RectTransform(mainFrame.RectTransform, new Vector2(0.7f, 0.7f), Anchor.Center, Pivot.Center), 25);
        new GUIText(new RectTransform(list.RectTransform, new Vector2(1f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), "Main Menu", 100, Color.White);
        var connectButton = new GUIButton(new RectTransform(list.RectTransform, new Vector2(0.5f, 0.1f), Anchor.TopCenter, Pivot.TopCenter), () =>
        {

        });
        connectButton.RectTransform.MinSize = new Vector2(400, 100);
        new GUIText(new RectTransform(connectButton.RectTransform, new Vector2(1f, 1f), Anchor.Center, Pivot.Center), "Connect Localhost", 50, Color.White);
    }

    public void OnDrawGUI(float deltaTime)
    {
        if (!EntityManager.World.AnyWith<MainMenuComponent>()) { return; }

        mainFrame.RectTransform.AbsoluteSize = new Vector2(WindowSystem.Width, WindowSystem.Height);

        RenderingSystem.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, WindowSystem.Width, WindowSystem.Height, 0f, 0.001f, 100f));
        mainFrame.Draw();
        RenderingSystem.End();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (!EntityManager.World.AnyWith<MainMenuComponent>()) { return; }

        mainFrame.Update();
    }
}