namespace KomunitasKampus.Domain.Entities;

public class ChatReadStatus : BaseEntity
{
    public Guid RoomId { get; set; }

    public Guid AccountId { get; set; }

    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public ChatRoom? Room { get; set; }

    public Account? Account { get; set; }
}