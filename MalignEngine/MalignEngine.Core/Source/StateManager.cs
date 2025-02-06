using System.Collections.Immutable;

namespace MalignEngine;

public interface IStateExit<T> : ISchedule
{
    void OnStateExit(T state);
}
public interface IStateEnter<T> : ISchedule
{
    void OnStateEnter(T state);
}

public class StateManager : IService
{
    public ImmutableDictionary<Type, Enum> States => states.ToImmutableDictionary();

    private Dictionary<Type, Enum> states = new();

    private ScheduleManager scheduleManager;

    public StateManager(ScheduleManager scheduleManager)
    {
        this.scheduleManager = scheduleManager;
    }

    public void Next<T>(T state) where T : Enum
    {
        if (states.ContainsKey(typeof(T)))
        {
            scheduleManager.Run<IStateExit<T>>(x => x.OnStateExit((T)states[typeof(T)]));
        }

        states[typeof(T)] = state;
        scheduleManager.Run<IStateEnter<T>>(x => x.OnStateEnter(state));
    }

    public Func<bool> Is<T>(T state) where T : Enum
    {
        return () =>
        {
            if (states.ContainsKey(typeof(T)) == false)
            {
                return false;
            }

            return states[typeof(T)].Equals(state);
        };
    }
}