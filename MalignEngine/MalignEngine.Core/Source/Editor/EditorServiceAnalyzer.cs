using ImGuiNET;
using System.Numerics;

namespace MalignEngine;

public class EditorServiceAnalyzer : BaseEditorWindowSystem
{
    public override string WindowName => "Service Analyzer";

    [Dependency]
    protected IRenderer2D RenderingService = default!;

    [Dependency]
    protected ScheduleManager ScheduleManager = default!;

    private Type selectedScheduleType;
    private RenderTexture renderTexture;

    private Type[] scheduleTypes;

    public override void OnInitialize()
    {
        base.OnInitialize();

        renderTexture = new RenderTexture(512, 512);
        selectedScheduleType = typeof(IInit);

        scheduleTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ISchedule).IsAssignableFrom(p) && p.IsInterface && p != typeof(ISchedule))
            .ToArray();
    }

    public override void DrawWindow(float delta)
    {
        ImGui.Begin("Service Analyzer");

        if (ImGui.BeginCombo("Schedule Type", selectedScheduleType.Name))
        {
            foreach (var type in scheduleTypes)
            {
                if (ImGui.Selectable(type.Name))
                {
                    selectedScheduleType = type;
                }
            }
            ImGui.EndCombo();
        }

        foreach (var schedule in ScheduleManager.GetOrder(selectedScheduleType))
        {
            ImGui.Text(schedule.Subscriber.GetType().Name);
        }

        //RenderingService.SetRenderTarget(renderTexture);

        //RenderingService.Begin(Matrix4x4.CreateOrthographicOffCenter(0f, 512, 512, 0f, 0.001f, 100f));
        //RenderingService.Clear(Color.Cyan);

        ////var graph = ScheduleManager.BuildGraph(typeof(IInit));

        //List<Vector2> takenSpots = new List<Vector2>();

        ////foreach (var vertex in graph.Vertices)
        ////{
        //    //RenderingService.DrawTexture2D(Texture2D.White, new Vector2(vertex.X, vertex.Y), new Vector2(50, 50), Color.Black);
        ////}

        //RenderingService.End();

        //RenderingService.SetRenderTarget(null);

        //ImGuiSystem.Image(renderTexture, new Vector2(512, 512));

        ImGui.End();
    }
}