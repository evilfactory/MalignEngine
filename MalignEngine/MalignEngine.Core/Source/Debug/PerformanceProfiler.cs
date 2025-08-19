using System.Diagnostics;

namespace MalignEngine;

public interface IPerformanceProfiler
{
    IDisposable BeginSample(string name);
    void EndSample();
    IEnumerable<ProfileStats> GetStats();
}

public struct ProfileSample
{
    public string Name;
    public double DurationMs;
    public int Depth;
}

public class ProfileHistory
{
    private readonly double[] _buffer;
    private int _index;
    private bool _filled;

    public ProfileHistory(int capacity)
    {
        _buffer = new double[capacity];
    }

    public void Add(double value)
    {
        _buffer[_index] = value;
        _index = (_index + 1) % _buffer.Length;
        if (_index == 0) _filled = true;
    }

    public IEnumerable<double> GetValues()
    {
        if (_filled)
        {
            for (int i = _index; i < _buffer.Length; i++)
            {
                yield return _buffer[i];
            }
            for (int i = 0; i < _index; i++)
            {
                yield return _buffer[i];
            }
        }
        else
        {
            {
                for (int i = 0; i < _index; i++) yield return _buffer[i];
            }
        }
    }
}

public class ProfileStats
{
    public string Name;
    public double LastMs;
    public double MinMs = double.MaxValue;
    public double MaxMs;
    public double TotalMs;
    public int Count;

    public ProfileHistory History;

    public ProfileStats(string name, int historySize = 120)
    {
        Name = name;
        History = new ProfileHistory(historySize);
    }

    public void Add(double durationMs)
    {
        LastMs = durationMs;
        MinMs = Math.Min(MinMs, durationMs);
        MaxMs = Math.Max(MaxMs, durationMs);
        TotalMs += durationMs;
        Count++;
        History.Add(durationMs);
    }

    public double AverageMs => Count > 0 ? TotalMs / Count : 0;
}

public class PerformanceProfiler : IService, IPerformanceProfiler
{
    private struct ActiveSample
    {
        public string Name;
        public Stopwatch Stopwatch;
        public int Depth;
    }

    private readonly Stack<ActiveSample> _stack = new();
    private readonly Dictionary<string, ProfileStats> _stats = new();

    public IDisposable BeginSample(string name)
    {
        var sample = new ActiveSample
        {
            Name = name,
            Stopwatch = Stopwatch.StartNew(),
            Depth = _stack.Count
        };
        _stack.Push(sample);
        return new SampleScope(this);
    }

    public void EndSample()
    {
        var sample = _stack.Pop();
        sample.Stopwatch.Stop();

        double ms = sample.Stopwatch.Elapsed.TotalMilliseconds;

        if (!_stats.TryGetValue(sample.Name, out var stat))
        {
            stat = new ProfileStats(sample.Name);
            _stats[sample.Name] = stat;
        }
        stat.Add(ms);
    }

    public IEnumerable<ProfileStats> GetStats() => _stats.Values;

    private readonly struct SampleScope : IDisposable
    {
        private readonly PerformanceProfiler _profiler;
        public SampleScope(PerformanceProfiler profiler) => _profiler = profiler;
        public void Dispose() => _profiler.EndSample();
    }
}