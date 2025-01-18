namespace MalignEngine;

public class HeadlessUpdateLoop : IService, IApplicationRun
{
    public int UpdateRate { get; set; } = 60;

    [Dependency]
    protected EventSystem EventSystem = default!;

    public void OnApplicationRun()
    {
        EventSystem.PublishEvent<IInit>(x => x.OnInitialize());

        while (true)
        {
            var startTime = DateTime.Now;

            EventSystem.PublishEvent<IPreUpdate>(x => x.OnPreUpdate(1.0f / UpdateRate));
            EventSystem.PublishEvent<IUpdate>(x => x.OnUpdate(1.0f / UpdateRate));
            EventSystem.PublishEvent<IPostUpdate>(x => x.OnPostUpdate(1.0f / UpdateRate));

            var endTime = DateTime.Now;
            var deltaTime = (endTime - startTime).TotalSeconds;

            if (deltaTime < 1.0f / UpdateRate)
            {
                Thread.Sleep((int)((1.0f / UpdateRate - deltaTime) * 1000));
            }
        }
    }
}