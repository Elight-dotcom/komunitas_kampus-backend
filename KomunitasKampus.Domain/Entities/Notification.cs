namespace KomunitasKampus.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid RecipientId { get; set; }

    public Guid? ActorId { get; set; }

    public string Type { get; set; } = string.Empty;

    public Guid? ReferenceId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public Account? Recipient { get; set; }

    public Account? Actor { get; set; }
}
