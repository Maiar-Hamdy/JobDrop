using System.Security.Claims;
using JobDrop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobDrop.WebApi.Middleware;

public sealed class TokenRevocationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, ApplicationDbContext db)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenVersionStr = ctx.User.FindFirstValue("token_version");

            if (Guid.TryParse(userIdStr, out var userId) && int.TryParse(tokenVersionStr, out var tokenVersion))
            {
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId, ctx.RequestAborted);

                if (user == null || !user.IsActive || user.TokenVersion != tokenVersion)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsJsonAsync(new 
                    { 
                        type = "urn:jobdrop:problem:token-revoked",
                        title = "Token revoked",
                        status = 401,
                        code = "TOKEN_REVOKED",
                        detail = "Session expired or token revoked."
                    });
                    return;
                }
            }
        }
        await next(ctx);
    }
}