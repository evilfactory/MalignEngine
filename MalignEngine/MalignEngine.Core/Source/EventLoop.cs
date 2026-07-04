using System.Diagnostics;
using System.Threading;

namespace MalignEngine;

public interface IEventLoop
{
    float TargetUpdateDelta { get; set; }
    float TargetDrawDelta { get; set; }
    void Start();
    void Tick();
    void Run();
    void Stop();
}

public class EventLoop : IEventLoop
{
    public float TargetUpdateDelta { get; set; } = 1f / 60f;
    public float TargetDrawDelta { get; set; } = 1f / 120f;

    private const double maxFrameTime = 0.25f;

    private double updateAccumulator = 0.0;
    private double drawAccumulator = 0.0;
    private double previousTime;
    private Stopwatch stopwatch = new Stopwatch();

    private bool _running = true;
    private IScheduleManager _scheduleManager;
    private IPerformanceProfiler? _performanceProfiler;

    public EventLoop(IScheduleManager scheduleManager, IPerformanceProfiler? performanceProfiler = null)
    {
        _scheduleManager = scheduleManager;
        _performanceProfiler = performanceProfiler;
    }

    public void Start()
    {
        stopwatch.Reset();
        stopwatch.Start();

        previousTime = stopwatch.Elapsed.TotalSeconds;
    }

    public void Tick()
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
    }

    public void Run()
    {
        Start();

        while (_running)
        {
            Tick();

            Thread.SpinWait(1);
        }
    }

    public void Stop() => _running = false;

    public void OnApplicationRun() => Run();
}
