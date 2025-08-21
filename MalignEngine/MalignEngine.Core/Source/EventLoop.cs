using System.Diagnostics;
using System.Threading;

namespace MalignEngine;

public interface IEventLoop
{
    float TargetUpdateDelta { get; set; }
    float TargetDrawDelta { get; set; }
    void Run();
    void Stop();
}

public class EventLoop : IService, IEventLoop, IApplicationRun
{
    public float TargetUpdateDelta { get; set; } = 1f / 60f;
    public float TargetDrawDelta { get; set; } = 1f / 120f;

    private bool _running = true;
    private IScheduleManager _scheduleManager;

    public EventLoop(IScheduleManager scheduleManager)
    {
        _scheduleManager = scheduleManager;
    }

    public void Run()
    {
        _scheduleManager.Run<IInit>(x => x.OnInitialize());

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        double previousTime = stopwatch.Elapsed.TotalSeconds;
        double updateAccumulator = 0.0;
        double drawAccumulator = 0.0;

        while (_running)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            double frameDelta = currentTime - previousTime;
            previousTime = currentTime;

            updateAccumulator += frameDelta;
            drawAccumulator += frameDelta;

            if (updateAccumulator >= TargetUpdateDelta)
            {
                _scheduleManager.Run<IPreUpdate>(e => e.OnPreUpdate((float)TargetUpdateDelta));
                _scheduleManager.Run<IUpdate>(e => e.OnUpdate((float)TargetUpdateDelta));
                _scheduleManager.Run<IPostUpdate>(e => e.OnPostUpdate((float)TargetUpdateDelta));

                updateAccumulator = 0;
            }

            if (drawAccumulator >= TargetDrawDelta)
            {
                Application.Main.ServiceContainer.GetInstance<ILogger>().LogVerbose(TargetDrawDelta + " > " + drawAccumulator);
                _scheduleManager.Run<IDraw>(x => x.OnDraw((float)drawAccumulator));
                drawAccumulator = 0;
            }

            Thread.Sleep(1);
        }
    }
    public void Stop() => _running = false;

    public void OnApplicationRun() => Run();
}
