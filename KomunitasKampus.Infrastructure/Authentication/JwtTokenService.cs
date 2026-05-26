using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Common.Models;
using KomunitasKampus.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KomunitasKampus.Infrastructure.Authentication;

public class JwtTokenService : IAuthTokenService
{
    private const string RefreshTokenType = "refresh";
    private const string AccessTokenType = "access";

    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public AuthTokenResult GenerateTokens(Account account)
    {
        var utcNow = DateTime.UtcNow;

        var accessTokenExpiresAt = utcNow.AddMinutes(
            _jwtSettings.AccessTokenExpirationMinutes
        );

        var refreshTokenExpiresAt = utcNow.AddDays(
            _jwtSettings.RefreshTokenExpirationDays
        );

        var accessToken = GenerateJwtToken(
            account,
            issuedAt: utcNow,
            expiresAt: accessTokenExpiresAt,
            tokenType: AccessTokenType
        );

        var refreshToken = GenerateJwtToken(
            account,
            issuedAt: utcNow,
            expiresAt: refreshTokenExpiresAt,
            tokenType: RefreshTokenType
        );

        return new AuthTokenResult(
            AccessToken: accessToken,
            AccessTokenExpiresAt: accessTokenExpiresAt,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );
    }

    public Guid? GetAccountIdFromRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                refreshToken,
                GetTokenValidationParameters(),
                out _
            );

            var tokenType = principal.FindFirst("token_type")?.Value;

            if (tokenType != RefreshTokenType)
            {
                return null;
            }

            var accountIdValue =
                principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(accountIdValue, out var accountId))
            {
                return null;
            }

            return accountId;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtToken(
        Account account,
        DateTime issuedAt,
        DateTime expiresAt,
        string tokenType
    )
    {
        var secretKey = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT Secret minimal harus 32 karakter."
            );
        }

        var roleValue = account.Role.ToString().Equals(
                "Organisasi",
                StringComparison.OrdinalIgnoreCase
            )
            ? "organization"
            : "user";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Username),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, roleValue),
            new("username", account.Username),
            new("role", roleValue),
            new("token_type", tokenType)
        };

        if (account.Organization is not null)
        {
            claims.Add(new Claim("organization_id", account.Organization.Id.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(secretKey);

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenValidationParameters GetTokenValidationParameters()
    {
        var secretKey = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}