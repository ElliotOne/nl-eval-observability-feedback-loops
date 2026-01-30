using System.Collections.Concurrent;

namespace EvalObservabilityFeedbackLoops;

public sealed record TraceSpan(
    string Name,
    double DurationMs,
    IReadOnlyDictionary<string, string> Attributes
);

public sealed record TelemetrySnapshot(
    int TotalSpans,
    double AverageLatencyMs,
    double P95LatencyMs,
    double AverageTokens,
    double AverageModelLatencyMs
);

public sealed class TelemetrySink
{
    private readonly List<TraceSpan> _spans = new();
    private readonly ConcurrentDictionary<string, List<double>> _metrics =
        new(StringComparer.OrdinalIgnoreCase);

    public void RecordSpan(
        string name,
        double durationMs,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        _spans.Add(new TraceSpan(
            name,
            durationMs,
            attributes ?? new Dictionary<string, string>()
        ));

        RecordMetric("latency_ms", durationMs);
    }

    public void RecordMetric(string name, double value)
    {
        var list = _metrics.GetOrAdd(name, _ => new List<double>());
        lock (list)
        {
            list.Add(value);
        }
    }

    public TelemetrySnapshot Snapshot()
    {
        var latency = GetMetricValues("latency_ms");
        var tokens = GetMetricValues("tokens");
        var modelLatency = GetMetricValues("llm.model_latency_ms");

        return new TelemetrySnapshot(
            TotalSpans: _spans.Count,
            AverageLatencyMs: Average(latency),
            P95LatencyMs: Percentile(latency, 95),
            AverageTokens: Average(tokens),
            AverageModelLatencyMs: Average(modelLatency)
        );
    }

    private IReadOnlyList<double> GetMetricValues(string name)
    {
        if (!_metrics.TryGetValue(name, out var list))
            return Array.Empty<double>();

        lock (list)
        {
            return list.ToArray();
        }
    }

    private static double Average(IReadOnlyList<double> values)
    {
        if (values.Count == 0) return 0;
        double sum = 0;
        for (var i = 0; i < values.Count; i++) sum += values[i];
        return sum / values.Count;
    }

    private static double Percentile(IReadOnlyList<double> values, int percentile)
    {
        if (values.Count == 0) return 0;

        var sorted = values.OrderBy(v => v).ToArray();

        // Linear interpolation percentile
        var p = percentile / 100.0;
        var pos = (sorted.Length - 1) * p;
        var lower = (int)Math.Floor(pos);
        var upper = (int)Math.Ceiling(pos);

        if (lower == upper) return sorted[lower];

        var weight = pos - lower;
        return sorted[lower] * (1 - weight) + sorted[upper] * weight;
    }
}
