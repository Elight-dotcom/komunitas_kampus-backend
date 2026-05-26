namespace KomunitasKampus.Application.Features.Auth.DTOs;

public sealed record LoginResponse(
    Guid AccountId,
    string Username,
    string Email,
    string Role,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    string TokenType
);