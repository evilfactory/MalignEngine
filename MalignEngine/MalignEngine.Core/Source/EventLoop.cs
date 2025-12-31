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

public class EventLoop : IEventLoop
{
    public float TargetUpdateDelta { get; set; } = 1f / 60f;
    public float TargetDrawDelta { get; set; } = 1f / 120f;

    private bool _running = true;
    private IScheduleManager _scheduleManager;
    private IPerformanceProfiler? _performanceProfiler;

    public EventLoop(IScheduleManager scheduleManager, IPerformanceProfiler? performanceProfiler = null)
    {
        _scheduleManager = scheduleManager;
        _performanceProfiler = performanceProfiler;
    }

    public void Run()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        double previousTime = stopwatch.Elapsed.TotalSeconds;
        double updateAccumulator = 0.0;
        double drawAccumulator = 0.0;
        const double maxFrameTime = 0.025;

        while (_running)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            double frameDelta = currentTime - previousTime;
            previousTime = currentTime;

            frameDelta = Math.Min(frameDelta, maxFrameTime);

            updateAccumulator += frameDelta;
            drawAccumulator += frameDelta;


            while (updateAccumulator >= TargetUpdateDelta)
            {
                _performanceProfiler?.BeginSample("update");
                _scheduleManager.Run<IPreUpdate>(e => e.OnPreUpdate((float)TargetUpdateDelta));
                _scheduleManager.Run<IUpdate>(e => e.OnUpdate((float)TargetUpdateDelta));
                _scheduleManager.Run<IPostUpdate>(e => e.OnPostUpdate((float)TargetUpdateDelta));
                _performanceProfiler?.EndSample();
                updateAccumulator -= TargetUpdateDelta;
            }

            if (drawAccumulator >= TargetDrawDelta)
            {
                _performanceProfiler?.BeginSample("draw");
                _scheduleManager.Run<IPreDraw>(x => x.OnPreDraw((float)drawAccumulator));
                _scheduleManager.Run<IDraw>(x => x.OnDraw((float)drawAccumulator));
                _scheduleManager.Run<IPostDraw>(x => x.OnPostDraw((float)drawAccumulator));
                _performanceProfiler?.EndSample();
                drawAccumulator = 0.0;
            }

            Thread.SpinWait(1);
        }
    }
    public void Stop() => _running = false;

    public void OnApplicationRun() => Run();
}
