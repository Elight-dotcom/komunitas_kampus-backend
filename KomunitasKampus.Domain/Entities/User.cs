namespace KomunitasKampus.Domain.Entities;

public class User : BaseEntity
{
    public Guid AccountId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string University { get; set; } = string.Empty;

    public Account? Account { get; set; }

    public ICollection<Like> Likes { get; set; } = new List<Like>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Share> Shares { get; set; } = new List<Share>();
}