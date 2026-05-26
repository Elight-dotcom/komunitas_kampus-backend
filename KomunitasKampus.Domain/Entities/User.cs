namespace KomunitasKampus.Domain.Entities;

public class User : BaseEntity
{
    public Guid AccountId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string University { get; set; } = string.Empty;

    public Account? Account { get; set; }
}