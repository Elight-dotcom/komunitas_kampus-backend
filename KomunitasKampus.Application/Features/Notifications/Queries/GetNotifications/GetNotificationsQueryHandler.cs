using KomunitasKampus.Application.Features.Notifications.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var notifications = await _notificationRepository.GetByRecipientAsync(
            request.RecipientId,
            cancellationToken
        );

        return notifications
            .Select(n => new NotificationDto(
                n.Id,
                n.RecipientId,
                n.ActorId,
                n.Actor?.User?.FullName ?? n.Actor?.Username ?? null,
                n.Actor?.AvatarUrl,
                n.Type,
                n.ReferenceId,
                n.IsRead,
                n.CreatedAt
            ))
            .ToList();
    }
}