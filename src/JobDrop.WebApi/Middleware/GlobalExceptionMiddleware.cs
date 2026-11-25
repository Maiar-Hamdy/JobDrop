using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using JobDrop.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace JobDrop.WebApi.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            var (status, code, title, detail) = ex switch
            {
                ValidationException ve => (400, "VALIDATION_FAILED", "One or more validation errors occurred.",
                    string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
                DomainRuleException de => (400, "DOMAIN_RULE", "Business rule violated.", de.Message),
                NotFoundException nf    => (404, "NOT_FOUND", "Resource not found.", nf.Message),
                ConcurrencyConflictException cc => (409, "CONCURRENCY_CONFLICT", "Concurrent modification detected.", cc.Message),
                UnauthorizedAccessException     => (401, "UNAUTHORIZED", "Unauthorized.", "Authentication required."),
                _                        => (500, "INTERNAL_ERROR", "An unexpected error occurred.", "Please contact support.")
            };

            if (status >= 500) logger.LogError(ex, "Unhandled exception");
            else logger.LogWarning(ex, "Handled exception {Code}", code);

            var problem = new ProblemDetails
            {
                Type = $"urn:jobdrop:problem:{code.ToLowerInvariant()}",
                Title = title, Status = status, Detail = detail,
                Instance = ctx.Request.Path
            };
            problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? ctx.TraceIdentifier;
            problem.Extensions["code"] = code;

            ctx.Response.Clear();
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}