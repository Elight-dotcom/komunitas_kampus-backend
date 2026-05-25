using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Auth.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthCommandResult>
{
    private readonly IAuthTokenService _authTokenService;
    private readonly IAccountRepository _accountRepository;

    public RefreshTokenCommandHandler(
        IAuthTokenService authTokenService,
        IAccountRepository accountRepository
    )
    {
        _authTokenService = authTokenService;
        _accountRepository = accountRepository;
    }

    public async Task<AuthCommandResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        var accountId = _authTokenService.GetAccountIdFromRefreshToken(
            request.RefreshToken
        );

        if (accountId is null)
        {
            throw new UnauthorizedAppException("Refresh token tidak valid.");
        }

        var account = await _accountRepository.GetByIdAsync(
            accountId.Value,
            cancellationToken
        );

        if (account is null || account.DeletedAt is not null)
        {
            throw new UnauthorizedAppException("Akun tidak ditemukan atau sudah tidak aktif.");
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