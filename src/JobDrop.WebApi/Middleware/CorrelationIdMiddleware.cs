namespace JobDrop.WebApi.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string Header = "X-Correlation-Id";
    public async Task InvokeAsync(HttpContext ctx)
    {
        var cid = ctx.Request.Headers.TryGetValue(Header, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString()
            : Guid.NewGuid().ToString("N");
        ctx.TraceIdentifier = cid;
        ctx.Response.Headers[Header] = cid;
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", cid))
            await next(ctx);
    }
}