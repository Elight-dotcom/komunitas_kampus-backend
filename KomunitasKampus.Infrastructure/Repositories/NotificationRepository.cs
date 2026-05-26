using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid recipientId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Notifications
            .AsNoTracking()
            .Include(n => n.Actor)
                .ThenInclude(a => a!.User)
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default
    )
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        if (notification is null) return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}