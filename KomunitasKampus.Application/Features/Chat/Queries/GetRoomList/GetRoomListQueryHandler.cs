using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Queries.GetRoomList;

public class GetRoomListQueryHandler : IRequestHandler<GetRoomListQuery, List<ChatRoomSummaryDto>>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatReadStatusRepository _chatReadStatusRepository;

    public GetRoomListQueryHandler(IChatRoomRepository chatRoomRepository, IChatReadStatusRepository chatReadStatusRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatReadStatusRepository = chatReadStatusRepository;
    }

    public async Task<List<ChatRoomSummaryDto>> Handle(GetRoomListQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _chatRoomRepository.GetRoomsByAccountIdAsync(request.AccountId, cancellationToken);
        var roomIds = rooms.Select(room => room.Id).ToList();
        var unreadCounts = await _chatReadStatusRepository.GetUnreadCountsAsync(request.AccountId, roomIds, cancellationToken);

        return rooms
            .Select(room => room.ToChatRoomSummaryDto(request.AccountId, unreadCounts.TryGetValue(room.Id, out var count) ? count : 0))
            .OrderByDescending(summary => summary.LastMessageAt ?? DateTime.MinValue)
            .ThenBy(summary => summary.Name)
            .ToList();
    }
}
