using System.Numerics;

namespace MalignEngine;

/*
public interface IAddToUpdateGUIList : ISchedule
{
    public void AddToUpdateList(GUIService gui);
}
public class GUIService : IService, IUpdate, IWindowDraw
{
    [Dependency]
    protected ScheduleManager ScheduleManager = default!;
    [Dependency]
    protected IRenderer2D RenderingService = default!;
    [Dependency]
    protected WindowService WindowSystem = default!;
    [Dependency]
    protected CameraSystem CameraSystem = default!;

    private List<GUIComponent> components = new List<GUIComponent>();

    private void SubscribeAllChildren(GUIComponent component)
    {
        foreach (var child in component.Children)
        {
            SubscribeAllChildren(child);
        }
    }

    public void AddToUpdateList(GUIComponent component)
    {
        SubscribeAllChildren(component);

        components.Add(component);
    }

    public void OnDrawGUI(float deltaTime)
    {
        foreach (var component in components)
        {
            RenderingService.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, WindowSystem.Width, WindowSystem.Height, 0f, 0.001f, 100f));
            component.Draw();
            RenderingService.End();
        }
    }

    public void OnUpdate(float deltaTime)
    {
        components.Clear();
        ScheduleManager.Run<IAddToUpdateGUIList>(x => x.AddToUpdateList(this));

        foreach (var component in components)
        {
            component.Update();
        }
    }

    public void OnWindowDraw(float deltaTime)
    {
        ScheduleManager.Run<IPreDrawGUI>(e => e.OnPreDrawGUI(deltaTime));

        foreach (var component in components)
        {
            RenderingService.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, WindowSystem.Width, WindowSystem.Height, 0f, 0.001f, 100f));
            component.Draw();
            RenderingService.End();
        }

        ScheduleManager.Run<IDrawGUI>(e => e.OnDrawGUI(deltaTime));
        ScheduleManager.Run<IPostDrawGUI>(e => e.OnPostDrawGUI(deltaTime));
    }
}
*/