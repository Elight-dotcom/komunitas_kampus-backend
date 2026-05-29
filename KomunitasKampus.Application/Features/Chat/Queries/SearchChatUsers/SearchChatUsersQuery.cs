using KomunitasKampus.Application.Features.Chat.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Queries.SearchChatUsers;

public sealed record SearchChatUsersQuery(
    Guid RequesterAccountId,
    string Username,
    int Limit = 10
) : IRequest<IReadOnlyList<ChatUserSearchResultDto>>;