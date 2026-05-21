using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class Post : BaseEntity
{
    public Guid OrganizationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public bool IsPinned { get; set; }

    public int? PinOrder { get; set; }

    public PostVisibility Visibility { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public int ShareCount { get; set; }

    public Organization? Organization { get; set; }

    public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();

    public ICollection<Like> Likes { get; set; } = new List<Like>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Share> Shares { get; set; } = new List<Share>();
}