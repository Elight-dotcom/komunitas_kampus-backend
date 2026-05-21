namespace KomunitasKampus.Domain.Entities;

public class Organization : BaseEntity
{
    public Guid AccountId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string University { get; set; } = string.Empty;

    public Account? Account { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public ICollection<Story> Stories { get; set; } = new List<Story>();

    public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
}