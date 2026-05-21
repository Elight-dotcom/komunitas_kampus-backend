namespace KomunitasKampus.Application.Common.Models;

public sealed record AuthTokenResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);