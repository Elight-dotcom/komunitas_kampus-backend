namespace KomunitasKampus.Application.Features.Auth.DTOs;

public sealed record RegisterOrganizationResponse(
    Guid AccountId,
    Guid OrganizationId,
    string Username,
    string Email,
    string Role,
    string OrganizationName,
    string University,
    string Slug
);