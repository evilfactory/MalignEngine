using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace MalignEngine
{
    public class EditorPerformanceSystem : BaseEditorWindowSystem
    {
        private const int MaxTicks = 120;

        private object mutex = new object();

        private Dictionary<string, Queue<long>> ticks = new Dictionary<string, Queue<long>>();

        private bool lag = false;
        private bool pause = false;
        private double updatesPerSecond;

        public override string WindowName => "Performance";

        public void AddElapsedTicks(string name, long tick)
        {
            if (pause) { return; }

            lock (mutex)
            {

                if (!ticks.ContainsKey(name))
                {
                    ticks[name] = new Queue<long>();
                }

                Queue<long> queue = ticks[name];

                if (queue.Count > MaxTicks - 1)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(tick);
            }
        }

        public override void Update(float deltatime)
        {
            updatesPerSecond = 1.0f / deltatime;

            if (lag)
            {
                for (int i = 0; i < 10000000; i++) { }
            }
        }

        public override void Draw(float deltatime)
        {
            if (!Active) { return; }

            ImGui.Begin("PerformanceDebugger");

            ImGui.Text($"FPS: {1.0f / deltatime}");
            ImGui.Text($"Delta Time: {deltatime}");
            ImGui.Text($"Updates Per Second: {updatesPerSecond}");
            
            ImGui.Checkbox("Pause", ref pause);
            ImGui.Checkbox("Lag", ref lag);

            foreach ((string name, Queue<long> queue) in ticks)
            {

                long[] queueValues = queue.ToArray();
                float[] values = new float[MaxTicks];
                float average = 0f;
                float maxValue = 0f;

                for (int i = 0; i < queueValues.Length; i++)
                {
                    values[i] = (float)((double)queueValues[i] / Stopwatch.Frequency);
                    average = average + values[i];

                    maxValue = MathF.Max(maxValue, values[i]);
                }

                average = average / queueValues.Length;

                Vector4 Color = Vector4.Lerp(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), average / 60);

                ImGui.PushStyleColor(ImGuiCol.PlotLines, Color);
                ImGui.PlotLines($"{name}\nAverage: {average}ms\nMax: {maxValue}ms", ref values[0], values.Length, 0, "", 0, 60, new Vector2(300, 50));
                ImGui.PopStyleColor();

            }

            ImGui.End();
        }
    }
}