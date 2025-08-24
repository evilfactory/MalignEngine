namespace MalignEngine;

public abstract class EventArgs { }

public interface IEventChannel<T>
{

}

public class EventChannel<T> : IEventChannel<T> where T : EventArgs
{
    private List<Action<T>> eventSubscriptions = new();

    public void Subscribe(Action<T> handler)
    {
        eventSubscriptions.Add(handler);
    }

    public void Raise(T args)
    {
        foreach (var subscription in eventSubscriptions)
        {
            if (subscription is Action<T> handler)
            {
                handler(args);
            }
        }
    }
}

public interface IEventService
{
    void Register<T>(IEventChannel<T> eventChannel);
    T Get<T>() where T : class;
}

public class EventService : IEventService, IService
{
    private List<object> eventChannels = new();

    public void Register<T>(IEventChannel<T> eventChannel)
    {
        eventChannels.Add(eventChannel);
    }

    public T Get<T>() where T : class
    {
        foreach (var channel in eventChannels)
        {
            if (channel is T eventChannel)
            {
                return eventChannel;
            }
        }

        return null;
    }
}