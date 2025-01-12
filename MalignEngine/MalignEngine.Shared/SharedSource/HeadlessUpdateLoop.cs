namespace MalignEngine;

public class HeadlessUpdateLoop : IService, IApplicationRun
{
    [Dependency]
    protected EventSystem EventSystem = default!;

    public void OnApplicationRun()
    {
        EventSystem.PublishEvent<IInit>(x => x.OnInitialize());

        // Update loop running at 60 updates per second
        while (true)
        {
            EventSystem.PublishEvent<IPreUpdate>(x => x.OnPreUpdate(1f / 60f));
            EventSystem.PublishEvent<IUpdate>(x => x.OnUpdate(1f / 60f));
            EventSystem.PublishEvent<IPostUpdate>(x => x.OnPostUpdate(1f / 60f));

            Thread.Sleep(1000 / 60);
        }
    }
}