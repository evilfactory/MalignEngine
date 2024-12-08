using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace MalignEngine
{
    public interface PerformanceLogData
    {
        public float Value();
        public float Average(IEnumerable<PerformanceLogData> values);
        public float MaxValue(IEnumerable<PerformanceLogData> values);
        public float MinValue(IEnumerable<PerformanceLogData> values);
    }

    public class StopWatchPerformanceLogData : PerformanceLogData
    {
        private long elapsedTicks;

        public StopWatchPerformanceLogData(long ticks)
        {
            elapsedTicks = ticks;
        }

        public float Average(IEnumerable<PerformanceLogData> values)
        {
            int count = 0;
            float average = 0f;
            foreach (StopWatchPerformanceLogData value in values)
            {
                average = average + value.Value();
                count = count + 1;
            }
            return average / count;
        }

        public float MaxValue(IEnumerable<PerformanceLogData> values)
        {
            float max = 0f;
            foreach (StopWatchPerformanceLogData value in values)
            {
                max = MathF.Max(max, value.Value());
            }
            return max;
        }

        public float MinValue(IEnumerable<PerformanceLogData> values)
        {
            float min = float.MaxValue;
            foreach (StopWatchPerformanceLogData value in values)
            {
                min = MathF.Min(min, value.Value());
            }
            return min;
        }

        public float Value()
        {
            return (float)elapsedTicks / Stopwatch.Frequency;
        }
    }

    public class FpsPerformanceData : PerformanceLogData
    {
        private float fps;

        public FpsPerformanceData(float fps)
        {
            this.fps = fps;
        }

        public float Average(IEnumerable<PerformanceLogData> values)
        {
            float average = 0f;
            foreach (FpsPerformanceData value in values)
            {
                average = average + value.Value();
            }
            return average / values.Count();
        }

        public float MaxValue(IEnumerable<PerformanceLogData> values)
        {
            float max = 0f;
            foreach (FpsPerformanceData value in values)
            {
                max = MathF.Max(max, value.Value());
            }
            return max;
        }

        public float MinValue(IEnumerable<PerformanceLogData> values)
        {
            float min = float.MaxValue;
            foreach (FpsPerformanceData value in values)
            {
                min = MathF.Min(min, value.Value());
            }
            return min;
        }

        public float Value()
        {
            return fps;
        }
    }

    public class EditorPerformanceSystem : BaseEditorWindowSystem
    {
        private const int MaxTicks = 360;

        private object mutex = new object();

        private Dictionary<string, Queue<PerformanceLogData>> performanceData = new Dictionary<string, Queue<PerformanceLogData>>();

        private bool lag = false;
        private bool pause = false;
        private double updatesPerSecond;

        public override string WindowName => "Performance";

        public void AddElapsedTicks(string name, PerformanceLogData data)
        {
            if (pause) { return; }

            lock (mutex)
            {

                if (!performanceData.ContainsKey(name))
                {
                    performanceData[name] = new Queue<PerformanceLogData>();
                }

                Queue<PerformanceLogData> queue = performanceData[name];

                if (queue.Count > MaxTicks - 1)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(data);
            }
        }

        public override void OnUpdate(float deltatime)
        {
            updatesPerSecond = 1.0f / deltatime;

            if (lag)
            {
                for (int i = 0; i < 10000000; i++) { }
            }
        }

        public override void DrawWindow(float deltatime)
        {
            ImGui.Begin("PerformanceDebugger");

            AddElapsedTicks("FPS", new FpsPerformanceData(1.0f / deltatime));

            ImGui.Text($"FPS: {1.0f / deltatime}");
            ImGui.Text($"Delta Time: {deltatime}");
            ImGui.Text($"Updates Per Second: {updatesPerSecond}");
            
            ImGui.Checkbox("Pause", ref pause);
            ImGui.Checkbox("Lag", ref lag);

            foreach ((string name, Queue<PerformanceLogData> queue) in performanceData)
            {

                PerformanceLogData[] queueValues = queue.ToArray();

                //Vector4 Color = Vector4.Lerp(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), average / 60);

                float[] values = new float[queueValues.Length];
                for (int i = 0; i < queueValues.Length; i++)
                {
                    values[i] = queueValues[i].Value();
                }

                //ImGui.PushStyleColor(ImGuiCol.PlotLines, Color);
                ImGui.PlotLines($"{name}\nAverage: {queueValues[0].Average(queueValues)}ms\nMax: {queueValues[0].MaxValue(queueValues)}ms", ref values[0], values.Length, 0, "", 0, queueValues[0].MaxValue(queueValues), new Vector2(300, 50));
                ImGui.PopStyleColor();

            }

            ImGui.End();
        }
    }
}