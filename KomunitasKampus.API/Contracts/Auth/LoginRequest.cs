namespace KomunitasKampus.API.Contracts.Auth;

public sealed record LoginRequest(
    string Identifier,
    string Password
);