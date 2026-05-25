using System.Text.RegularExpressions;
using KomunitasKampus.Application.Features.Auth.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.RegisterOrganization;

public class RegisterOrganizationCommandHandler
    : IRequestHandler<RegisterOrganizationCommand, RegisterOrganizationResponse>
{
    private readonly IAccountRepository _accountRepository;

    public RegisterOrganizationCommandHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<RegisterOrganizationResponse> Handle(
        RegisterOrganizationCommand request,
        CancellationToken cancellationToken
    )
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var organizationName = request.OrganizationName.Trim();
        var university = request.University.Trim();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var slug = GenerateSlug(organizationName, university);

        var organization = new Organization
        {
            OrganizationName = organizationName,
            University = university,
            Slug = slug,
            Description = string.Empty
        };

        var account = new Account
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = AccountRole.Organisasi,
            AvatarUrl = null,
            Organization = organization
        };

        organization.Account = account;
        organization.AccountId = account.Id;

        await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return new RegisterOrganizationResponse(
            AccountId: account.Id,
            OrganizationId: organization.Id,
            Username: account.Username,
            Email: account.Email,
            Role: account.Role.ToString(),
            OrganizationName: organization.OrganizationName,
            University: organization.University,
            Slug: organization.Slug
        );
    }

    private static string GenerateSlug(string organizationName, string university)
    {
        var rawSlug = $"{organizationName}.{university}".Trim().ToLowerInvariant();

        var normalizedSlug = Regex.Replace(rawSlug, @"[^a-z0-9]+", ".");

        return normalizedSlug.Trim('.');
    }
}