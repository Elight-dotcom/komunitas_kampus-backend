namespace KomunitasKampus.Domain.Entities;

public class ChatParticipant : BaseEntity
{
    public Guid RoomId { get; set; }

    public Guid AccountId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ChatRoom? Room { get; set; }

    public Account? Account { get; set; }
}