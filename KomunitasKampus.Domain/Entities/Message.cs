namespace KomunitasKampus.Domain.Entities;

public class Message : BaseEntity
{
    public Guid RoomId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public Guid? DeletedById { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public ChatRoom? Room { get; set; }

    public Account? Sender { get; set; }

    public Account? DeletedBy { get; set; }
}