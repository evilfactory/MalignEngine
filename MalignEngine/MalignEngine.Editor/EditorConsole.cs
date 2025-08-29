using ImGuiNET;
using System.Numerics;

namespace MalignEngine.Editor;

public class EditorConsole : BaseEditorWindowSystem, ILogHandler
{
    [Dependency]
    protected ILoggerService LoggerService = default!;

    private class DebugConsoleLog
    {
        public string Line;
        public Vector4 Color;

        public DebugConsoleLog(string line, Vector4 color)
        {
            Line = line;
            Color = color;
        }
    }

    public override string WindowName => "Console";

    private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();

    private List<DebugConsoleLog> logs = new List<DebugConsoleLog>();

    private bool autoScroll = true;
    private string command = "";

    public EditorConsole(EditorSystem editorSystem, ImGuiService imGuiService, ILoggerService loggerService) : base(editorSystem, imGuiService)
    {
        loggerService.Root.AddHandler(this);
    }

    public override void DrawWindow(float delta)
    {
        ImGui.Begin("DebugConsole");

        if (ImGui.Button("Clear"))
        {
            logs = new List<DebugConsoleLog>();
        }

        ImGui.SameLine();


        ImGuiInputTextFlags textFlags = ImGuiInputTextFlags.EnterReturnsTrue;
        if (ImGui.InputText("Command", ref command, 250, textFlags))
        {
            ExecuteCommand(command);
            command = "";
        }

        ImGui.SameLine();
        ImGui.Checkbox("Auto-scroll", ref autoScroll);

        ImGui.Separator();
        ImGui.BeginChild("scrolling", new Vector2(0, 0), false);

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));

        for (int i = 0; i < logs.Count; i++)
        {
            DebugConsoleLog log = logs[i];

            ImGui.PushStyleColor(ImGuiCol.Text, log.Color);

            ImGui.TextUnformatted($"{i}: {log.Line}");

            ImGui.PopStyleColor();
        }

        if (autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
        {
            ImGui.SetScrollHereY(1.0f);
        }

        ImGui.PopStyleVar();

        ImGui.EndChild();

        ImGui.End();
    }


    public void NewLog(LogType type, string line)
    {
        Vector4 color = new Vector4(1f, 1f, 1f, 1f);

        switch (type)
        {
            case LogType.Verbose:
                color = new Vector4(0.8f, 0.2f, 0.8f, 1f);
                break;

            case LogType.Info:
                color = new Vector4(0.2f, 0.2f, 0.9f, 1f);
                break;

            case LogType.Warning:
                color = new Vector4(1f, 1f, 0f, 1f);
                break;

            case LogType.Error:
                color = new Vector4(1f, 0f, 0f, 1f);
                break;
        }

        NewLog(line, color);
    }

    public void NewLog(string line, Vector4 color)
    {
        // Limit the number of logs to 1000
        if (logs.Count > 1000)
        {
            logs.RemoveAt(0);
        }

        logs.Add(new DebugConsoleLog(line, color));
    }

    public void AddCommand(string command, Action<string[]> action)
    {
        commands.Add(command, action);
    }

    public static string[] SplitCommand(string command)
    {
        command = command.Trim();

        List<string> commands = new List<string>();
        int escape = 0;
        bool inQuotes = false;
        string piece = "";

        for (int i = 0; i < command.Length; i++)
        {
            if (command[i] == '\\')
            {
                if (escape == 0) escape = 2;
                else piece += '\\';
            }
            else if (command[i] == '"')
            {
                if (escape == 0) inQuotes = !inQuotes;
                else piece += '"';
            }
            else if (command[i] == ' ' && !inQuotes)
            {
                if (!string.IsNullOrWhiteSpace(piece)) commands.Add(piece);
                piece = "";
            }
            else if (escape == 0) piece += command[i];

            if (escape > 0) escape--;
        }

        if (!string.IsNullOrWhiteSpace(piece)) commands.Add(piece); //add final piece

        return commands.ToArray();
    }

    public void ExecuteCommand(string command)
    {
        LoggerService.LogInfo($"Console command: {command}");

        string[] args = SplitCommand(command);

        if (commands.ContainsKey(args[0]))
        {
            commands[args[0]](args);
        }
    }

    public void HandleLog(Sawmill sawmill, LogEvent logEvent)
    {
        NewLog(logEvent.LogType, $"{sawmill.Name}: {logEvent.Message}");
    }
}
