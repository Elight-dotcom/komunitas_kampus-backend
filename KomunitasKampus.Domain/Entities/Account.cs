using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class Account : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public AccountRole Role { get; set; }

    public string? AvatarUrl { get; set; }

    public Organization? Organization { get; set; }

    public User? User { get; set; }

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public ICollection<Comment> DeletedComments { get; set; } = new List<Comment>();

    public ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();

    public ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();

    public ICollection<Message> DeletedMessages { get; set; } = new List<Message>();

    public ICollection<ChatReadStatus> ChatReadStatuses { get; set; } = new List<ChatReadStatus>();
}