using KomunitasKampus.Application.Features.Auth.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IAccountRepository _accountRepository;

    public RegisterUserCommandHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken
    )
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var fullName = request.FullName.Trim();
        var university = request.University.Trim();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FullName = fullName,
            University = university
        };

        var account = new Account
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = AccountRole.Mahasiswa,
            AvatarUrl = null,
            User = user
        };

        user.Account = account;
        user.AccountId = account.Id;

        await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(
            AccountId: account.Id,
            UserId: user.Id,
            Username: account.Username,
            Email: account.Email,
            Role: account.Role.ToString(),
            FullName: user.FullName,
            University: user.University
        );
    }
}