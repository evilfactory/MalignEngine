namespace MalignEngine;

/// <summary>
/// Basic system with an Update and Draw schedule
/// </summary>
public abstract class BaseSystem : ISystem, IUpdate, IDraw, IDisposable
{
    protected readonly ILoggerService LoggerService;
    protected readonly IScheduleManager ScheduleManager;

    protected virtual ILogger Logger => LoggerService.GetSawmill(GetType().Name);

    public BaseSystem(IServiceContainer serviceContainer)
    {
        LoggerService = serviceContainer.GetInstance<ILoggerService>();
        ScheduleManager = serviceContainer.GetInstance<IScheduleManager>();

        ScheduleManager.RegisterAll(this);
    }

    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnDraw(float deltaTime) { }

    public virtual void Dispose()
    {
        ScheduleManager.UnregisterAll(this);
    }
}