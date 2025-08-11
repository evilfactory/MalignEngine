using QuikGraph;
using QuikGraph.Algorithms;
using System.Collections.Immutable;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;

namespace MalignEngine;

public interface ISchedule { }

public class ScheduleMetaData
{
    public float Priority { get; set; }
    public Func<bool>? RunCondition { get; set; }

    public ScheduleMetaData()
    {
        Priority = 0;
    }

    public ScheduleMetaData(Func<bool> runCondition)
    {
        RunCondition = runCondition;
    }

    public ScheduleMetaData(float priority, Func<bool> runCondition)
    {
        Priority = priority;
        RunCondition = runCondition;
    }
}

public interface IScheduleManager
{
    public void Run<T>(Action<T> action) where T : ISchedule;
    public void SetMetaData<TSchedule, TService>(ScheduleMetaData metadata);
}

/// <summary>
/// Manages the order of execution of all schedules using high-performance topological sorting.
/// </summary>
public class ScheduleManager : IScheduleManager, IService
{
    private IServiceContainer serviceContainer;

    private readonly ConcurrentDictionary<(Type, Type), ScheduleMetaData> _metadata
        = new ConcurrentDictionary<(Type, Type), ScheduleMetaData>();

    public ScheduleManager(IServiceContainer serviceContainer)
    {
        this.serviceContainer = serviceContainer;
    }

    public void Run<T>(Action<T> action) where T : ISchedule
    {
        var sortedSchedules = serviceContainer.GetInstances<T>().ToList();
        sortedSchedules.Sort((x, y) => 
        {
            var px = GetMetaData(typeof(T), x.GetType()).Priority;
            var py = GetMetaData(typeof(T), y.GetType()).Priority;
            return px.CompareTo(py);
        });

        foreach (T schedule in sortedSchedules)
        {
            var metadata = GetMetaData(typeof(T), schedule.GetType());
            if (metadata.RunCondition != null && !metadata.RunCondition())
            {
                continue;
            }

            action(schedule);
        }
    }

    private ScheduleMetaData GetMetaData(Type scheduleType, Type serviceType)
    {
        var key = (scheduleType, serviceType);

        if (_metadata.TryGetValue(key, out var val))
        {
            return val;
        }

        var metadata = new ScheduleMetaData();

        foreach (var stageAttribute in serviceType.GetCustomAttributes<StageAttribute>())
        {
            if (stageAttribute.ScheduleType == scheduleType)
            {
                metadata.Priority = stageAttribute.Priority;
                break;
            }
        }

        return metadata;
    }

    public void SetMetaData<TSchedule, TService>(ScheduleMetaData metadata)
    {
        var key = (typeof(TSchedule), typeof(TService));
        _metadata.AddOrUpdate(key, metadata, (k, v) => metadata);

        InvalidateCache();
    }

    private void InvalidateCache()
    {

    }

    public void ClearAllCaches()
    {
        _metadata.Clear();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class StageAttribute : Attribute
{
    /// <summary>
    /// 0.0 - Lowest priority
    /// 1.0 - Highest priority
    /// </summary>
    public float Priority { get; protected set; }
    public Type ScheduleType { get; protected set; }

    public StageAttribute(Type scheduleType, float priority)
    {
        ScheduleType = scheduleType;
        Priority = priority;
    }

}

public class StageAttribute<TSchedule> : StageAttribute where TSchedule : ISchedule
{
    public StageAttribute(float priority) : base(typeof(TSchedule), priority) { }
}

public class StageAttribute<TSchedule, TStage> : StageAttribute<TSchedule> where TStage : Stage, new() where TSchedule : ISchedule
{
    public StageAttribute() : base(new TStage().Priority) { }
}

public abstract class Stage
{
    public abstract float Priority { get; }
}

public class HighestPriorityStage : Stage
{
    public override float Priority => 1f;
}

public class LowestPriorityStage : Stage
{
    public override float Priority => 0f;
}