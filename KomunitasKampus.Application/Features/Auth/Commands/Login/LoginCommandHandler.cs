using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Auth.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthCommandResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAuthTokenService _authTokenService;

    public LoginCommandHandler(
        IAccountRepository accountRepository,
        IAuthTokenService authTokenService
    )
    {
        _accountRepository = accountRepository;
        _authTokenService = authTokenService;
    }

    public async Task<AuthCommandResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
    {
        var identifier = request.Identifier.Trim();

        var account = await _accountRepository.GetByEmailOrUsernameAsync(
            identifier,
            cancellationToken
        );

        if (account is null || account.DeletedAt is not null)
        {
            throw new UnauthorizedAppException();
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            account.PasswordHash
        );

        if (!isPasswordValid)
        {
            throw new UnauthorizedAppException();
        }

        var tokenResult = _authTokenService.GenerateTokens(account);

        var response = new LoginResponse(
            AccountId: account.Id,
            Username: account.Username,
            Email: account.Email,
            Role: account.Role.ToString(),
            AccessToken: tokenResult.AccessToken,
            AccessTokenExpiresAt: tokenResult.AccessTokenExpiresAt,
            RefreshTokenExpiresAt: tokenResult.RefreshTokenExpiresAt,
            TokenType: "Bearer"
        );

        return new AuthCommandResult(
            Response: response,
            RefreshToken: tokenResult.RefreshToken,
            RefreshTokenExpiresAt: tokenResult.RefreshTokenExpiresAt
        );
    }
}