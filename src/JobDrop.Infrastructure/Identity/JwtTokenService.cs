using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JobDrop.Domain.Entities.Identity;
using JobDrop.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JobDrop.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtOptions> opts)
{
    private readonly JwtOptions _o = opts.Value;

    public (string accessToken, DateTime expiresUtc) CreateAccessToken(AppIdentityUser user, IEnumerable<string> roles)
    {
        var now = DateTime.UtcNow;
        var exp = now.AddMinutes(_o.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("token_version", user.TokenVersion.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_o.Issuer, _o.Audience, claims, now, exp, creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), exp);
    }

    public (string rawToken, string hash, UserSession session) CreateRefreshToken(Guid userId, Guid? familyId = null)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        var session = new UserSession
        {
            UserId = userId,
            RefreshTokenHash = hash,
            FamilyId = familyId ?? Guid.NewGuid(),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_o.RefreshTokenDays)
        };
        return (raw, hash, session);
    }

    public static string HashRefreshToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}