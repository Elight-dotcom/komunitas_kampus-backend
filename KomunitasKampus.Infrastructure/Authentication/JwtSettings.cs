namespace KomunitasKampus.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; init; } = 15;

    public int RefreshTokenExpirationDays { get; init; } = 7;

    public string RefreshTokenCookieName { get; init; } = "refreshToken";

    public bool CookieSecure { get; init; } = true;

    public string CookieSameSite { get; init; } = "None";
}