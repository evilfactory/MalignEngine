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

public class EventLoop : IEventLoop, IApplicationClosing
{
    public float TargetUpdateDelta { get; set; } = 1f / 60f;
    public float TargetDrawDelta { get; set; } = 1f / 120f;

    public ExecutionPipeline? _updatePipeline { get; set; }
    public ExecutionPipeline? _drawPipeline { get; set; }

    private const double maxFrameTime = 0.25f;

    private double updateAccumulator = 0.0;
    private double drawAccumulator = 0.0;
    private double previousTime;
    private Stopwatch stopwatch = new Stopwatch();

    private bool _running = true;
    private IScheduleManager _scheduleManager;
    private IPerformanceProfiler? _performanceProfiler;

    public EventLoop(IScheduleManager scheduleManager, ExecutionPipeline? updatePipeline = null, ExecutionPipeline? drawPipeline = null, IPerformanceProfiler? performanceProfiler = null)
    {
        _updatePipeline = updatePipeline;
        _drawPipeline = drawPipeline;
        _performanceProfiler = performanceProfiler;
        _scheduleManager = scheduleManager;

        _scheduleManager.Register<IApplicationClosing>(this);
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
            _updatePipeline?.Execute(new ExecutionContext(_scheduleManager, TargetUpdateDelta));
            _performanceProfiler?.EndSample();
            updateAccumulator -= TargetUpdateDelta;
        }

        if (drawAccumulator >= TargetDrawDelta)
        {
            _performanceProfiler?.BeginSample("draw");
            _drawPipeline?.Execute(new ExecutionContext(_scheduleManager, drawAccumulator));
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

    public void OnApplicationClosing()
    {
        Stop();
    }
}
