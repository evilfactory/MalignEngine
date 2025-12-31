namespace MalignEngine;

/// <summary>
/// Basic system with an Update and Draw schedule
/// </summary>
public abstract class BaseSystem : ISystem, IUpdate, IDraw, IDisposable
{
    protected readonly ILogger Logger;
    protected readonly IScheduleManager ScheduleManager;

    public BaseSystem(ILoggerService loggerService, IScheduleManager scheduleManager)
    {
        Logger = loggerService.GetSawmill(GetType().Name);
        ScheduleManager = scheduleManager;

        ScheduleManager.RegisterAll(this);
    }

    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnDraw(float deltaTime) { }

    public virtual void Dispose()
    {
        ScheduleManager.UnregisterAll(this);
    }
}