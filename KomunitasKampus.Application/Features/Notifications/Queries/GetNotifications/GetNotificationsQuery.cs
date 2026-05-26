using KomunitasKampus.Application.Features.Notifications.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(Guid RecipientId)
    : IRequest<IReadOnlyList<NotificationDto>>;