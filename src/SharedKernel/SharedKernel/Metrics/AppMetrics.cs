using Prometheus;

namespace SharedKernel.Metrics;

public class AppMetrics
{
    public static readonly Counter Errors = Prometheus.Metrics.CreateCounter("error_count", "This counter displays the error count.");
}
