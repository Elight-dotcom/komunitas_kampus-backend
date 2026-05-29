using KomunitasKampus.Application.Features.Auth.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Queries.CheckUsernameAvailability;

public class CheckUsernameAvailabilityQueryHandler : IRequestHandler<CheckUsernameAvailabilityQuery, UsernameAvailabilityDto>
{
    private readonly IAccountRepository _accountRepository;

    public CheckUsernameAvailabilityQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<UsernameAvailabilityDto> Handle(CheckUsernameAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var normalizedUsername = request.Username.Trim();
        var isAvailable = !await _accountRepository.IsUsernameTakenAsync(normalizedUsername, cancellationToken);

        return new UsernameAvailabilityDto(
            Username: normalizedUsername,
            Available: isAvailable
        );
    }
}