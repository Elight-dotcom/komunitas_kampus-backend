using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Infrastructure.Persistence;
using KomunitasKampus.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<AppHub> _hubContext;

    public NotificationService(
        AppDbContext context,
        IHubContext<AppHub> hubContext
    )
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendMembershipNotificationAsync(
        Guid accountId,
        Guid organizationId,
        string notificationType,
        CancellationToken cancellationToken = default
    )
    {
        var organizationActorId = await _context.Organizations
            .Where(organization => organization.Id == organizationId)
            .Select(organization => (Guid?)organization.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        await SendNotificationAsync(
            recipientId: accountId,
            actorId: organizationActorId,
            type: notificationType,
            referenceId: organizationId,
            cancellationToken: cancellationToken
        );
    }

    public async Task SendNotificationAsync(
        Guid recipientId,
        Guid? actorId,
        string type,
        Guid? referenceId,
        CancellationToken cancellationToken = default
    )
    {
        var notification = new Notification
        {
            RecipientId = recipientId,
            ActorId = actorId,
            Type = type,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients
            .Group(RealtimeGroups.Account(recipientId))
            .SendAsync(
                "notification_received",
                new
                {
                    id = notification.Id,
                    recipientId = notification.RecipientId,
                    actorId = notification.ActorId,
                    type = notification.Type,
                    referenceId = notification.ReferenceId,
                    isRead = notification.IsRead,
                    createdAt = notification.CreatedAt
                },
                cancellationToken
            );
    }
}
