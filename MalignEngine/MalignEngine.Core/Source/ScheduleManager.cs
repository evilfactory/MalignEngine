using QuikGraph;
using QuikGraph.Algorithms;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace MalignEngine;

public interface ISchedule { }

public class ScheduleMetaData
{
    public Type[] RunsBefore { get; } = new Type[0];
    public Type[] RunsAfter { get; } = new Type[0];
    public Func<bool>? RunCondition { get; set; }

    public ScheduleMetaData(Type[] runsAfter, Type[] runsBefore)
    {
        RunsAfter = runsAfter;
        RunsBefore = runsBefore;
    }

    public ScheduleMetaData(Func<bool> runCondition)
    {
        RunCondition = runCondition;
    }

    public ScheduleMetaData(Type[] runsAfter, Type[] runsBefore, Func<bool> runCondition)
    {
        RunsAfter = runsAfter;
        RunsBefore = runsBefore;
        RunCondition = runCondition;
    }

    public ScheduleMetaData() { }
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

    private readonly ConcurrentDictionary<Type, ImmutableList<object>> _sortedCache
        = new ConcurrentDictionary<Type, ImmutableList<object>>();

    private readonly ConcurrentDictionary<(Type, Type), ScheduleMetaData> _metadata
        = new ConcurrentDictionary<(Type, Type), ScheduleMetaData>();

    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>> _typeInstanceCache
        = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>>();

    public ScheduleManager(IServiceContainer serviceContainer)
    {
        this.serviceContainer = serviceContainer;
    }

    public void Run<T>(Action<T> action) where T : ISchedule
    {
        //var sortedSchedules = GetTopologicallySortedSchedules<T>();
        var sortedSchedules = serviceContainer.GetInstances<T>().ToList();

        foreach (T schedule in sortedSchedules)
        {
            var metadata = GetMetaData(typeof(T), schedule.GetType());
            if (metadata?.RunCondition != null && !metadata.RunCondition())
            {
                continue;
            }

            action(schedule);
        }
    }

    private ImmutableList<T> GetTopologicallySortedSchedules<T>() where T : ISchedule
    {
        var cacheKey = typeof(T);

        if (_sortedCache.TryGetValue(cacheKey, out var cached))
        {
            return cached.Cast<T>().ToImmutableList();
        }

        var instances = serviceContainer.GetInstances<T>().ToList();
        CacheTypeInstances<T>(instances);

        var sorted = TopologicalSortQuikGraph(instances);

        var immutableSorted = sorted.ToImmutableList();
        _sortedCache.TryAdd(cacheKey, immutableSorted.Cast<object>().ToImmutableList());

        return immutableSorted;
    }

    private void CacheTypeInstances<T>(List<T> instances) where T : ISchedule
    {
        var typeCache = new ConcurrentDictionary<Type, object>();
        foreach (var instance in instances)
        {
            typeCache.TryAdd(instance.GetType(), instance);
        }
        _typeInstanceCache.AddOrUpdate(typeof(T), typeCache, (k, v) => typeCache);
    }

    private List<T> TopologicalSortQuikGraph<T>(List<T> schedules) where T : ISchedule
    {
        if (schedules.Count <= 1)
        {
            return schedules;
        }

        var graph = new AdjacencyGraph<T, Edge<T>>();

        graph.AddVertexRange(schedules);

        foreach (var schedule in schedules)
        {
            var metadata = GetMetaData(typeof(T), schedule.GetType());
            if (metadata == null) { continue; }

            foreach (var afterType in metadata.RunsAfter)
            {
                var afterSchedule = FindScheduleByType<T>(afterType);
                if (afterSchedule != null)
                {
                    graph.AddEdge(new Edge<T>(afterSchedule, schedule));
                }
            }

            foreach (var beforeType in metadata.RunsBefore)
            {
                var beforeSchedule = FindScheduleByType<T>(beforeType);
                if (beforeSchedule != null)
                {
                    graph.AddEdge(new Edge<T>(schedule, beforeSchedule));
                }
            }
        }

        try
        {
            var sortedVertices = graph.TopologicalSort();
            return sortedVertices.ToList();
        }
        catch (NonAcyclicGraphException)
        {
            throw new InvalidOperationException($"Circular dependency detected in schedule type {typeof(T).Name}");
        }
    }

    private T? FindScheduleByType<T>(Type targetType) where T : ISchedule
    {
        var baseType = typeof(T);
        if (_typeInstanceCache.TryGetValue(baseType, out var typeCache) &&
            typeCache.TryGetValue(targetType, out var instance))
        {
            return (T)instance;
        }
        return default(T);
    }

    private ScheduleMetaData? GetMetaData(Type scheduleType, Type serviceType)
    {
        var key = (scheduleType, serviceType);

        if (_metadata.TryGetValue(key, out var val))
        {
            return val;
        }

        return null;
    }

    public void SetMetaData<TSchedule, TService>(ScheduleMetaData metadata)
    {
        var key = (typeof(TSchedule), typeof(TService));
        _metadata.AddOrUpdate(key, metadata, (k, v) => metadata);

        InvalidateCache();
    }

    private void InvalidateCache()
    {
        _sortedCache.Clear();
        _typeInstanceCache.Clear();
    }

    public void ClearAllCaches()
    {
        _sortedCache.Clear();
        _metadata.Clear();
        _typeInstanceCache.Clear();
    }
}