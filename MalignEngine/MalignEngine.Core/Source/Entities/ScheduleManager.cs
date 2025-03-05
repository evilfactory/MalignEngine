using QuikGraph;
using QuikGraph.Algorithms;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;

namespace MalignEngine;

public interface IScheduleSubscriber { }


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

public class Schedule
{
    public Type SubscriberType { get; set; }
    public object? Subscriber { get; set; }
    public ScheduleMetaData? MetaData { get; set; }
}

/// <summary>
/// Instantiated by Application, this class manages the order of execution of all schedules.
/// </summary>
public class ScheduleManager : IService
{
    private Dictionary<Type, List<Schedule>> subscribers = new();

    public ScheduleManager()
    {
    }

    public ImmutableList<Schedule> GetOrder(Type type)
    {
        if (subscribers.ContainsKey(type) == false) 
        {
            return ImmutableList<Schedule>.Empty; 
        }

        return subscribers[type].ToImmutableList();
    }

    public AdjacencyGraph<Type, Edge<Type>> BuildGraph(Type type)
    {
        var graph = new AdjacencyGraph<Type, Edge<Type>>();

        List<Schedule> schedules = subscribers[type];

        foreach (var schedule in schedules)
        {
            graph.AddVertex(schedule.SubscriberType);
        }

        Type? prevSchedule = null; 
        foreach (var schedule in schedules)
        {
            ScheduleMetaData scheduleMetaData = schedule.MetaData;
            if (scheduleMetaData == null && prevSchedule != null)
            {
                scheduleMetaData = new ScheduleMetaData(new Type[] { prevSchedule }, new Type[0]);
            }

            prevSchedule = schedule.SubscriberType;

            if (scheduleMetaData == null) { continue; }

            foreach (var before in scheduleMetaData.RunsBefore)
            {
                graph.AddEdge(new Edge<Type>(schedule.SubscriberType, before));
            }

            foreach (var after in scheduleMetaData.RunsAfter)
            {
                graph.AddEdge(new Edge<Type>(after, schedule.SubscriberType));
            }
        }

        return graph;
    }

    private void ReorderAll(Type type)
    {
        var graph = BuildGraph(type);

        var topologicalSort = graph.TopologicalSort();

        if (topologicalSort == null)
        {
            throw new Exception("Circular dependency detected in schedule order");
        }

        List<Schedule> sortedSubscribers = new List<Schedule>();

        foreach (var vertex in topologicalSort)
        {
            sortedSubscribers.Add(subscribers[type].First(schedule => schedule.SubscriberType == vertex));
        }

        subscribers[type] = sortedSubscribers;
    }

    public void SubscribeAll(object observer)
    {
        foreach (var type in observer.GetType().GetInterfaces().Where(inter => inter.IsAssignableTo(typeof(ISchedule))))
        {
            if (!subscribers.ContainsKey(type))
            {
                subscribers.Add(type, new List<Schedule>());
            }

            if (subscribers[type].Contains(observer)) { return; }

            Schedule? schedule = subscribers[type].FirstOrDefault(sub => sub.SubscriberType == observer.GetType());

            if (schedule == null)
            {
                schedule = new Schedule() { MetaData = null, SubscriberType = observer.GetType() };
                subscribers[type].Add(schedule);
            }

            schedule.Subscriber = observer;

            ReorderAll(type);
        }
    }

    public void SetMetaData(Type scheduleType, Type subscriberType, ScheduleMetaData metadata)
    {
        if (!subscribers.ContainsKey(scheduleType))
        {
            subscribers[scheduleType] = new List<Schedule>();
        }

        Schedule? schedule = subscribers[scheduleType].FirstOrDefault(sub => sub.SubscriberType == subscriberType);

        if (schedule == null)
        {
            schedule = new Schedule() { SubscriberType = subscriberType, MetaData = metadata };

            subscribers[scheduleType].Add(schedule);
        }

        schedule.MetaData = metadata;
    }

    public void SetMetaData<TSchedule, TSubscriber>(ScheduleMetaData metadata) where TSchedule : ISchedule
    {
        SetMetaData(typeof(TSchedule), typeof(TSubscriber), metadata);
    }

    public void Run<T>(Action<T> eventInvoker) where T : ISchedule
    {
        if (!subscribers.ContainsKey(typeof(T))) { return; }

        foreach (var subscriber in subscribers[typeof(T)])
        {
            if (subscriber.Subscriber != null && (subscriber.MetaData == null || subscriber.MetaData.RunCondition == null || subscriber.MetaData.RunCondition()))
            {
                eventInvoker((T)subscriber.Subscriber);
            }
        }

        return;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ScheduleOrder : Attribute
{
    public Type[] RunsBefore { get; }
    public Type[] RunsAfter { get; }

    public ScheduleOrder(Type[] runsAfter, Type[] runsBefore)
    {
        RunsAfter = runsAfter;
        RunsBefore = runsBefore;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ScheduleOrder<T> : ScheduleOrder
{
    public ScheduleOrder(Type[] runsAfter, Type[] runsBefore) : base(runsAfter, runsBefore) { }
}