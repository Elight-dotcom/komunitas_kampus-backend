namespace KomunitasKampus.Application.Features.Auth.DTOs;

public sealed record RegisterUserResponse(
    Guid AccountId,
    Guid UserId,
    string Username,
    string Email,
    string Role,
    string FullName,
    string University
);