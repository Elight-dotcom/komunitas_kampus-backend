namespace KomunitasKampus.Domain.Entities;

public class Share : BaseEntity
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public Post? Post { get; set; }

    public User? User { get; set; }
}