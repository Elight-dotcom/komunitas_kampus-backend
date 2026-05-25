namespace KomunitasKampus.Application.Features.Auth.DTOs;

public sealed record AuthCommandResult(
    LoginResponse Response,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);