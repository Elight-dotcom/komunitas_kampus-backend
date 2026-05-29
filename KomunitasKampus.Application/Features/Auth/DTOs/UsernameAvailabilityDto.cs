namespace KomunitasKampus.Application.Features.Auth.DTOs;

public sealed record UsernameAvailabilityDto(
    string Username,
    bool Available
);