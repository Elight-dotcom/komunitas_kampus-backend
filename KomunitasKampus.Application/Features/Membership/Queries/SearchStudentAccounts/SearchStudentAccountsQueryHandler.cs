using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.SearchStudentAccounts;

public class SearchStudentAccountsQueryHandler : IRequestHandler<SearchStudentAccountsQuery, IReadOnlyList<StudentSearchResultDto>>
{
    private readonly IAccountRepository _accountRepository;

    public SearchStudentAccountsQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IReadOnlyList<StudentSearchResultDto>> Handle(SearchStudentAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.SearchStudentAccountsByUsernameAsync(
            request.Username,
            request.Limit,
            cancellationToken
        );

        return accounts.Select(account => new StudentSearchResultDto(
            AccountId: account.Id,
            Username: account.Username,
            FullName: account.User?.FullName,
            University: account.User?.University,
            AvatarUrl: account.AvatarUrl
        )).ToList();
    }
}