using MediatR;
using Microsoft.Extensions.Logging;

namespace JobDrop.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", name);
        try { return await next(); }
        catch (Exception ex) { logger.LogError(ex, "Handler {RequestName} failed", name); throw; }
    }
}