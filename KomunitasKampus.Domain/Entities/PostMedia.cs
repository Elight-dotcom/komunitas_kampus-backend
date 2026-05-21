using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class PostMedia : BaseEntity
{
    public Guid PostId { get; set; }

    public PostMediaType MediaType { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public int FileSizeBytes { get; set; }

    public int OrderIndex { get; set; }

    public PostMediaStatus Status { get; set; }

    public Post? Post { get; set; }
}