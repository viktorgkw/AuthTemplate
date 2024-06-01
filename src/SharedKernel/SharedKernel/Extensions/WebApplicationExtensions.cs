using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;

namespace SharedKernel.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// This extension configures the /health and /metrics endpoints.
    /// </summary>
    public static WebApplication UseTelemetry(this WebApplication app)
    {
        app.UseMetricServer();

        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
