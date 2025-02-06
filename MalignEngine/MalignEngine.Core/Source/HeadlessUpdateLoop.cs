namespace MalignEngine;

public class HeadlessUpdateLoop : IService, IApplicationRun, IUpdateLoop
{
    public double UpdateRate { get; set; } = 60;

    [Dependency]
    protected ScheduleManager EventSystem = default!;

    public void OnApplicationRun()
    {
        EventSystem.Run<IInit>(x => x.OnInitialize());

        while (true)
        {
            var startTime = DateTime.Now;

            EventSystem.Run<IPreUpdate>(x => x.OnPreUpdate(1.0f / (float)UpdateRate));
            EventSystem.Run<IUpdate>(x => x.OnUpdate(1.0f / (float)UpdateRate));
            EventSystem.Run<IPostUpdate>(x => x.OnPostUpdate(1.0f / (float)UpdateRate));

            var endTime = DateTime.Now;
            var deltaTime = (endTime - startTime).TotalSeconds;

            if (deltaTime < 1.0f / UpdateRate)
            {
                Thread.Sleep((int)((1.0f / UpdateRate - deltaTime) * 1000));
            }
        }
    }
}