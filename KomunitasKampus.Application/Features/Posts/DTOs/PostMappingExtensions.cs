using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Application.Features.Posts.DTOs;

public static class PostMappingExtensions
{
    public static PostDto ToDto(this Post post)
    {
        return new PostDto(
            Id: post.Id,
            OrganizationId: post.OrganizationId,
            OrganizationName: post.Organization?.OrganizationName ?? string.Empty,
            OrganizationAvatarUrl: null, // Organization entity doesn't have AvatarUrl
            Title: post.Title,
            Caption: post.Caption,
            IsPinned: post.IsPinned,
            PinOrder: post.PinOrder,
            Visibility: post.Visibility.ToString(),
            LikeCount: post.LikeCount,
            CommentCount: post.CommentCount,
            ShareCount: post.ShareCount,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt,
            Media: post.Media
                .OrderBy(media => media.OrderIndex)
                .Select(media => new PostMediaDto(
                    Id: media.Id,
                    MediaType: media.MediaType.ToString(),
                    FileUrl: media.FileUrl,
                    FileSizeBytes: media.FileSizeBytes,
                    OrderIndex: media.OrderIndex,
                    Status: media.Status.ToString()
                ))
                .ToList()
        );
    }
}
