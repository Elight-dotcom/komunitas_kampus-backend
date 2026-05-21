using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public Guid? OrganizationId { get; set; }

    public string? Name { get; set; }

    public bool IsMainGroup { get; set; }

    public ChatRoomType RoomType { get; set; }

    public bool IsInviteOnly { get; set; }

    public Organization? Organization { get; set; }

    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<ChatReadStatus> ReadStatuses { get; set; } = new List<ChatReadStatus>();
}