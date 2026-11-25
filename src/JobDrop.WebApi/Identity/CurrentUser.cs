using System.Security.Claims;
using JobDrop.Application.Common;

namespace JobDrop.WebApi.Identity;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? P => accessor.HttpContext?.User;
    public Guid? UserId => Guid.TryParse(P?.FindFirstValue(ClaimTypes.NameIdentifier) ?? P?.FindFirstValue("sub"), out var id) ? id : null;
    public string? Email => P?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => P?.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => P?.IsInRole(role) ?? false;
    public int TokenVersion => int.TryParse(P?.FindFirstValue("token_version"), out var v) ? v : 0;
    public string? CorrelationId => accessor.HttpContext?.TraceIdentifier;
}