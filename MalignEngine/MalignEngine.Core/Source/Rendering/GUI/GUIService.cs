using System.Numerics;

namespace MalignEngine;

public interface IAddToUpdateGUIList : ISchedule
{
    public void AddToUpdateList(GUIService gui);
}

public class GUIService : IService, IUpdate, IDrawGUI
{
    [Dependency]
    protected ScheduleManager EventSystem = default!;

    [Dependency]
    protected IRenderingService RenderingService = default!;

    [Dependency]
    protected WindowSystem WindowSystem = default!;

    private List<GUIComponent> components = new List<GUIComponent>();

    private void SubscribeAllChildren(GUIComponent component)
    {
        foreach (var child in component.Children)
        {
            EventSystem.SubscribeAll(child);

            SubscribeAllChildren(child);
        }
    }

    public void AddToUpdateList(GUIComponent component)
    {
        EventSystem.SubscribeAll(component);
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
        EventSystem.Run<IAddToUpdateGUIList>(x => x.AddToUpdateList(this));

        foreach (var component in components)
        {
            component.Update();
        }
    }
}