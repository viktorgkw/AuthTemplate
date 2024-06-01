using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SharedKernel.MediatR;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();

        try
        {
            _logger.LogInformation($"[START] -> {typeof(TRequest).Name}");
            stopwatch.Start();

            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation($"[FINISH] - [{stopwatch.Elapsed.TotalSeconds:F2}] -> {typeof(TResponse).Name}");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError($"[ERROR] - [{stopwatch.Elapsed.TotalSeconds:F2}] -> {typeof(TResponse).Name} -> {ex.Message}");

            throw;
        }
    }
}
