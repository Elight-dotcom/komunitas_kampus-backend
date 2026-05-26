namespace KomunitasKampus.Domain.Entities;

public class StoryView : BaseEntity
{
    public Guid StoryId { get; set; }

    public Guid AccountId { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public Story? Story { get; set; }

    public Account? Account { get; set; }
}