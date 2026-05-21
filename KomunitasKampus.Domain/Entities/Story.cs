using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class Story : BaseEntity
{
    public Guid OrganizationId { get; set; }

    public StoryMediaType MediaType { get; set; }

    public string? MediaUrl { get; set; }

    public string? TextContent { get; set; }

    public bool IsExpired { get; set; }

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

    public Organization? Organization { get; set; }

    public ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();
}