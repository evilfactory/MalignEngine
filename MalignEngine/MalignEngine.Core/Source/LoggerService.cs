namespace MalignEngine;

public enum LogType
{
    Verbose,
    Info,
    Warning,
    Error,
}

public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogVerbose(string message);
    void LogError(string message);

    void Log(LogType type, string message);
}

public interface ILoggerService : ILogger
{
    Sawmill Root { get; }
    ILogger GetSawmill(string name);
}

public interface ILogHandler
{
    void HandleLog(Sawmill sawmill, LogEvent logEvent);
}

public class LogEvent
{
    public LogType LogType;
    public string Message;

    public LogEvent(LogType logType, string message)
    {
        LogType = logType;
        Message = message;
    }
}

public class Sawmill : ILogger
{
    public Sawmill? Parent { get; set; }
    public string Name { get; private set; }

    private List<ILogHandler> handlers = new List<ILogHandler>();

    public Sawmill(string name)
    {
        Name = name;
    }

    public void AddHandler(ILogHandler handler) => handlers.Add(handler);
    public void RemoveHandler(ILogHandler handler) => handlers.Remove(handler);

    public void LogWarning(string message) => Log(LogType.Warning, message);
    public void LogInfo(string message) => Log(LogType.Info, message);
    public void LogVerbose(string message) => Log(LogType.Verbose, message);
    public void LogError(string message) => Log(LogType.Error, message);

    public void Log(LogType type, string message)
    {
        LogInternal(this, type, message);
    }

    private void LogInternal(Sawmill sawmill, LogType type, string message)
    {
        foreach (var handler in handlers)
        {
            handler.HandleLog(sawmill, new LogEvent(type, message));
        }

        if (Parent != null)
        {
            Parent.LogInternal(sawmill, type, message);
        }
    }
}

public class LoggerService : IService, ILoggerService
{
    public Sawmill Root { get; private set; } = new Sawmill("root");
    private Dictionary<string, Sawmill> sawmills = new Dictionary<string, Sawmill>();

    public ILogger GetSawmill(string name)
    {
        if (!sawmills.ContainsKey(name))
        {
            sawmills.Add(name, new Sawmill(name) { Parent = Root });
        }

        return sawmills[name];
    }

    public void LogInfo(string message) => Log(LogType.Info, message);
    public void LogVerbose(string message) => Log(LogType.Verbose, message);
    public void LogWarning(string message) => Log(LogType.Warning, message);
    public void LogError(string message) => Log(LogType.Error, message);
    public void Log(LogType type, string message) => Root.Log(type, message);
}