using System.Data;
using System.Collections.Concurrent;
using System.Reflection;

namespace MalignEngine;

public interface ISchedule { }

public interface IScheduleManager
{
    public void Register(Type interfaceType, object listener);
    public void Unregister(Type interfaceType, object listener);
    public void Register<T>(object listener) where T : ISchedule;
    public void Unregister<T>(object listener) where T : ISchedule;
    public void RegisterAll(object listener);
    public void UnregisterAll(object listener);
    public void Run<T>(Action<T> action) where T : ISchedule;
    public void SetMetaData<TSchedule, TService>(ScheduleMetaData metadata);
}

public class ScheduleMetaData
{
    public float Priority { get; set; } = 0.5f;
    public Func<bool>? RunCondition { get; set; }

    public ScheduleMetaData()
    {
        Priority = 0.5f;
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

/// <summary>
/// Manages the order of execution of all schedules.
/// </summary>
public class ScheduleManager : IScheduleManager, IService
{
    private readonly ConcurrentDictionary<(Type, Type), ScheduleMetaData> _metadata
        = new ConcurrentDictionary<(Type, Type), ScheduleMetaData>();

    private ConcurrentDictionary<Type, List<object>> listeners = 
        new ConcurrentDictionary<Type, List<object>>();

    public ScheduleManager() { }

    public void Register(Type interfaceType, object listener)
    {
        if (interfaceType.IsAssignableFrom(typeof(ISchedule)))
        {
            throw new InvalidOperationException("Tried to register a type that doesn't implement ISchedule");
        }

        List<object> interfaceListeners = listeners.GetOrAdd(interfaceType, new List<object>());
        interfaceListeners.Add(listener);
    }

    public void Unregister(Type interfaceType, object listener)
    {
        if (interfaceType.IsAssignableFrom(typeof(ISchedule)))
        {
            throw new InvalidOperationException("Tried to unregister a type that doesn't implement ISchedule");
        }


        List<object> interfaceListeners = listeners.GetOrAdd(interfaceType, new List<object>());
        interfaceListeners.Remove(listener);
    }

    public void Register<T>(object listener) where T : ISchedule 
        => Register(typeof(T), listener);

    public void Unregister<T>(object listener) where T : ISchedule 
        => Register(typeof(T), listener);

    public void RegisterAll(object listener)
    {
        Type type = listener.GetType();
        IEnumerable<Type> interfaces = type.GetInterfaces().Where(x => x != typeof(ISchedule) && x.IsAssignableTo(typeof(ISchedule)));

        foreach (Type schedule in interfaces)
        {
            Register(schedule, listener);
        }
    }

    public void UnregisterAll(object listener)
    {
        Type type = listener.GetType();
        IEnumerable<Type> interfaces = type.GetInterfaces().Where(x => x != typeof(ISchedule) && x.IsAssignableTo(typeof(ISchedule)));

        foreach (Type schedule in interfaces)
        {
            Unregister(schedule, listener);
        }
    }

    public void Run<T>(Action<T> action) where T : ISchedule
    {
        Type scheduleType = typeof(T);

        if (!listeners.TryGetValue(scheduleType, out List<object>? sortedSchedules))
        {
            return;
        }

        sortedSchedules.Sort((x, y) => 
        {
            var px = GetMetaData(scheduleType, x.GetType()).Priority;
            var py = GetMetaData(scheduleType, y.GetType()).Priority;
            return px.CompareTo(py);
        });

        foreach (T schedule in sortedSchedules)
        {
            var metadata = GetMetaData(scheduleType, schedule.GetType());
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
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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