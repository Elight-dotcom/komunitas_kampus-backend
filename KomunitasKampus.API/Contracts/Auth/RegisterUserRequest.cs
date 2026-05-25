namespace KomunitasKampus.API.Contracts.Auth;

public sealed record RegisterUserRequest(
    string FullName,
    string University,
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
);