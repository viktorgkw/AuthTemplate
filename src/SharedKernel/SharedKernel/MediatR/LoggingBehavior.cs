using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Metrics;
using SharedKernel.Models;
using System.Diagnostics;

namespace SharedKernel.MediatR;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    RequestCorrelationId requestCorrelationId) : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly RequestCorrelationId _requestCorrelationId = requestCorrelationId;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();

        try
        {
            _logger.LogInformation($"[{_requestCorrelationId.Id}] Starting {typeof(TRequest).Name}");
            stopwatch.Start();

            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation($"[{_requestCorrelationId.Id}] Finished {typeof(TResponse).Name} in {stopwatch.Elapsed.TotalSeconds:F2}s");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError($"[{_requestCorrelationId.Id}] Error {typeof(TResponse).Name} in [{stopwatch.Elapsed.TotalSeconds:F2}] -> {ex.Message}");

            AppMetrics.Errors.Inc();

            throw;
        }
    }
}
