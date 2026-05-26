namespace KomunitasKampus.API.Contracts.Auth;

public sealed record RegisterOrganizationRequest(
    string OrganizationName,
    string University,
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
);