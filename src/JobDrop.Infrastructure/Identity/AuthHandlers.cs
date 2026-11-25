using JobDrop.Application.Contracts.Persistence;
using JobDrop.Application.Features.Auth;
using JobDrop.Domain.Entities.Candidates;
using JobDrop.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobDrop.Infrastructure.Identity;

public sealed class RegisterHandler(
    UserManager<AppIdentityUser> userManager,
    RoleManager<AppIdentityRole> roleManager,
    IApplicationDbContext db,
    ILogger<RegisterHandler> logger)
    : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken ct)
    {
        var user = new AppIdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            Handle = request.Handle,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) 
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var role = request.IsRecruiter ? "CompanyOwner" : "JobSeeker";
        
        // Ensure role exists (seed if missing)
        if (!await roleManager.RoleExistsAsync(role))
        {
            logger.LogWarning("Role {Role} missing. Seeding it now.", role);
            await roleManager.CreateAsync(new AppIdentityRole { Name = role });
        }
        
        await userManager.AddToRoleAsync(user, role);

        if (!request.IsRecruiter)
        {
            db.CandidateProfiles.Add(new CandidateProfile { UserId = user.Id });
        }

        await db.SaveChangesAsync(ct);
        return user.Id;
    }
}

public sealed class LoginHandler(
    UserManager<AppIdentityUser> userManager,
    SignInManager<AppIdentityUser> signInManager,
    JwtTokenService jwtService,
    IApplicationDbContext db)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        
        if (result.IsLockedOut) throw new UnauthorizedAccessException("Account locked out.");
        if (!result.Succeeded) throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresUtc) = jwtService.CreateAccessToken(user, roles);
        var (rawRefresh, hash, session) = jwtService.CreateRefreshToken(user.Id);

        db.UserSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, rawRefresh, expiresUtc);
    }
}

public sealed class RefreshHandler(
    IApplicationDbContext db,
    JwtTokenService jwtService,
    UserManager<AppIdentityUser> userManager)
    : IRequestHandler<RefreshCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshCommand request, CancellationToken ct)
    {
        var hash = JwtTokenService.HashRefreshToken(request.RefreshToken);
        var session = await db.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!session.IsActive(DateTime.UtcNow))
        {
            var familySessions = await db.UserSessions.Where(s => s.FamilyId == session.FamilyId).ToListAsync(ct);
            foreach (var s in familySessions) { s.RevokedAtUtc = DateTime.UtcNow; }
            await db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Session expired or revoked.");
        }

        var user = await userManager.FindByIdAsync(session.UserId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        session.RevokedAtUtc = DateTime.UtcNow;
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresUtc) = jwtService.CreateAccessToken(user, roles);
        var (rawRefresh, newHash, newSession) = jwtService.CreateRefreshToken(user.Id, session.FamilyId);

        db.UserSessions.Add(newSession);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, rawRefresh, expiresUtc);
    }
}