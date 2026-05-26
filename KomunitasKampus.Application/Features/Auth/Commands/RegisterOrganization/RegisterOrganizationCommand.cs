using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.RegisterOrganization;

public sealed record RegisterOrganizationCommand(
    string OrganizationName,
    string University,
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterOrganizationResponse>;