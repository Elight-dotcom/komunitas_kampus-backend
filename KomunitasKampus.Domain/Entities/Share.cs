using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class Share : BaseEntity
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public SharePlatform Platform { get; set; }

    public Post? Post { get; set; }

    public Account? User { get; set; }
}