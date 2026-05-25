namespace KomunitasKampus.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? DeletedReason { get; set; }

    public Guid? DeletedById { get; set; }

    public Post? Post { get; set; }

    public Account? User { get; set; }

    public Account? DeletedBy { get; set; }
}