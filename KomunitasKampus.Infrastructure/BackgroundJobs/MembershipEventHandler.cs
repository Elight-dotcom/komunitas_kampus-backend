using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace KomunitasKampus.Infrastructure.BackgroundJobs;

public class MembershipEventHandler : IMembershipEventPublisher
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IHubContext<AppHub> _hubContext;

    public MembershipEventHandler(
        IChatRoomRepository chatRoomRepository,
        IHubContext<AppHub> hubContext
    )
    {
        _chatRoomRepository = chatRoomRepository;
        _hubContext = hubContext;
    }

    public async Task PublishMemberAcceptedAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        var mainGroupRoom = await _chatRoomRepository.GetMainGroupByOrganizationIdAsync(
            organizationId,
            cancellationToken
        );

        if (mainGroupRoom is null)
        {
            return;
        }

        await _chatRoomRepository.AddParticipantAsync(
            new ChatParticipant
            {
                RoomId = mainGroupRoom.Id,
                AccountId = accountId,
                JoinedAt = DateTime.UtcNow
            },
            cancellationToken
        );

        await _hubContext
            .Clients
            .User(accountId.ToString())
            .SendAsync(
                "RoomListUpdated",
                cancellationToken
            );
    }
}
