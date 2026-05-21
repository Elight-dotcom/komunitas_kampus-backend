using KomunitasKampus.Application.Common.Models;
using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Application.Common.Interfaces;

public interface IAuthTokenService
{
    AuthTokenResult GenerateTokens(Account account);

    Guid? GetAccountIdFromRefreshToken(string refreshToken);
}