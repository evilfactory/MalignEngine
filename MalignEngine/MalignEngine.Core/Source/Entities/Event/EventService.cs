namespace MalignEngine;

public interface IEvent { }

public interface IEventService
{
    void Send<T>(T evt) where T : IEvent;
    EventReader<T> GetReader<T>() where T : IEvent;
}

interface IEventChannel
{
    void Trim(int count);
}

class EventChannel<T> : IEventChannel
{
    public List<T> Events = new();

    public void Trim(int count)
    {
        Events.RemoveRange(0, count);
    }
}

public class EventReader
{
    public int NextIndex { get; internal set; }
}

public class EventReader<T> : EventReader
{
    private EventChannel<T> Channel;

    internal EventReader(EventChannel<T> channel)
    {
        Channel = channel;
    }

    public IEnumerable<T> Read()
    {
        while (NextIndex < Channel.Events.Count)
        {
            yield return Channel.Events[NextIndex++];
        }
    }
}

public class EventService : IEventService, IService, IPreUpdate
{
    private readonly Dictionary<Type, IEventChannel> _channels = [];
    private readonly Dictionary<Type, List<EventReader>> _eventReaders = [];

    public EventService(IScheduleManager scheduleManager)
    {
        scheduleManager.RegisterAll(this);
    }

    private EventChannel<T> GetChannel<T>()
    {
        if (!_channels.TryGetValue(typeof(T), out var channel))
        {
            channel = new EventChannel<T>();
            _channels.Add(typeof(T), channel);
        }

        return (EventChannel<T>)channel;
    }

    public void Send<T>(T evt) where T : IEvent
    {
        GetChannel<T>().Events.Add(evt);
    }

    public EventReader<T> GetReader<T>() where T : IEvent
    {
        EventReader<T> reader = new EventReader<T>(GetChannel<T>());
        if (!_eventReaders.TryGetValue(typeof(T), out List<EventReader>? readerList))
        {
            _eventReaders.Add(typeof(T), [reader]);
        }
        else
        {
            readerList.Add(reader);
        }

        return reader;
    }

    public void OnPreUpdate(float deltaTime)
    {
        foreach (var (type, readers) in _eventReaders)
        {
            int lowest = readers.Min(reader => reader.NextIndex);
            
            if (lowest > 0)
            {
                _channels[type].Trim(lowest);

                foreach (var reader in readers)
                {
                    reader.NextIndex -= lowest;
                }
            }
        }

    }
}