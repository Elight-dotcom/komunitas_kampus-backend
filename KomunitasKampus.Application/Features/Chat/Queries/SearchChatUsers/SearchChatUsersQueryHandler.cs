using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Queries.SearchChatUsers;

public class SearchChatUsersQueryHandler : IRequestHandler<SearchChatUsersQuery, IReadOnlyList<ChatUserSearchResultDto>>
{
    private readonly IAccountRepository _accountRepository;

    public SearchChatUsersQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IReadOnlyList<ChatUserSearchResultDto>> Handle(
        SearchChatUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var accounts = await _accountRepository.SearchStudentAccountsByUsernameAsync(
            request.Username,
            request.Limit + 1,
            cancellationToken
        );

        return accounts
            .Where(account => account.Id != request.RequesterAccountId)
            .Take(request.Limit)
            .Select(account => new ChatUserSearchResultDto(
                AccountId: account.Id,
                Username: account.Username,
                FullName: account.User?.FullName,
                University: account.User?.University,
                AvatarUrl: account.AvatarUrl
            ))
            .ToList();
    }
}