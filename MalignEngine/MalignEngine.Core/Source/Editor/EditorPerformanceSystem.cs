using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace MalignEngine;

public class EditorPerformanceSystem : BaseEditorWindowSystem
{
    private IPerformanceProfiler _performanceProfiler;
    private bool _lag = false;
    private double _updatesPerSecond;

    public override string WindowName => "Performance Profiler";

    public EditorPerformanceSystem(EditorSystem editorSystem, ImGuiService imGuiService, IPerformanceProfiler performanceProfiler) : base(editorSystem, imGuiService)
    {
        _performanceProfiler = performanceProfiler;
    }

    public override void OnUpdate(float deltatime)
    {
        _updatesPerSecond = 1.0f / deltatime;

        if (_lag)
        {
            for (int i = 0; i < 10000000; i++) { }
        }
    }

    public override void DrawWindow(float deltatime)
    {
        ImGui.Begin("PerformanceDebugger");

        ImGui.Text($"FPS: {1.0f / deltatime}");
        ImGui.Text($"Delta Time: {deltatime}");
        ImGui.Text($"Updates Per Second: {_updatesPerSecond}");
        
        ImGui.Checkbox("Lag", ref _lag);

        foreach (var stat in _performanceProfiler.GetStats())
        {
            ImGui.Text($"{stat.Name}: {stat.LastMs:F2} ms (avg {stat.AverageMs:F2} / min {stat.MinMs:F2} / max {stat.MaxMs:F2})");

            var values = stat.History.GetValues().ToArray();
            if (values.Length > 0)
            {
                var doubles = stat.History.GetValues().ToArray();
                var floats = Array.ConvertAll(doubles, d => (float)d);
                ImGui.PlotLines("##profile", ref floats[0], values.Length);
            }
        }

        ImGui.End();
    }
}