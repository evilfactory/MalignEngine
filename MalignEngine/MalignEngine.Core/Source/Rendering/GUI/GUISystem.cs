using System.Numerics;

namespace MalignEngine;

public interface IAddToUpdateGUIList : ISchedule
{
    public void AddToUpdateList(GUISystem gui);
}


[Stage<IPostDraw, HighestPriorityStage>]
public class GUISystem : ISystem, IUpdate, IPostDraw, IDisposable
{
    private List<GUIComponent> components = new List<GUIComponent>();

    private readonly IScheduleManager _scheduleManager;
    private readonly IRenderingAPI _renderingAPI;
    private readonly IRenderer2D _renderer2D;
    private readonly IWindowService _window;

    public GUISystem(IScheduleManager scheduleManager, IRenderingAPI renderingAPI, IRenderer2D renderer2D, IWindowService window)
    {
        _scheduleManager = scheduleManager;
        _renderer2D = renderer2D;
        _window = window;
        _renderingAPI = renderingAPI;

        _scheduleManager.RegisterAll(this);
    }

    public void AddToUpdateList(GUIComponent component)
    {
        components.Add(component);
    }

    public void OnUpdate(float deltaTime)
    {
        components.Clear();
        _scheduleManager.Run<IAddToUpdateGUIList>(x => x.AddToUpdateList(this));

        foreach (var component in components)
        {
            component.Update();
        }
    }

    public void OnPostDraw(float deltaTime)
    {
        _scheduleManager.Run<IPreDrawGUI>(e => e.OnPreDrawGUI(deltaTime));

        _renderingAPI.Submit(ctx =>
        {
            lock(components)
            {
                _renderer2D.Begin(ctx, Matrix4x4.CreateOrthographicOffCenter(0f, _window.FrameSize.X, _window.FrameSize.Y, 0f, 0.001f, 100f));
                foreach (var component in components)
                {
                    component.Draw();
                }
                _renderer2D.End();
            }
        });

        _scheduleManager.Run<IDrawGUI>(e => e.OnDrawGUI(deltaTime));
        _scheduleManager.Run<IPostDrawGUI>(e => e.OnPostDrawGUI(deltaTime));
    }

    public void Dispose()
    {
        _scheduleManager.UnregisterAll(this);
    }
}