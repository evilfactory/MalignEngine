namespace MalignEngine
{
    public enum LogType
    {
        Verbose,
        Info,
        Warning,
        Error,
    }

    public class Logger
    {
        public static event Action<LogType, string> NewLog;

        public static void LogWarning(string message) => Log(LogType.Warning, message);
        public static void LogInfo(string message) => Log(LogType.Info, message);
        public static void LogVerbose(string message) => Log(LogType.Verbose, message);
        public static void LogError(string message) => Log(LogType.Error, message);

        public static void Log(LogType type, string message)
        {
            NewLog?.Invoke(type, message);

            switch (type)
            {
                case LogType.Verbose:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[VERBOSE] {message}");
                    break;

                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"[INFO] {message}");
                    break;

                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] {message}");
                    break;

                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] {message}");
                    break;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}