using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Application.Features.Interactions;

public static class InteractionMappingExtensions
{
    public static CommentDto ToCommentDto(this Comment comment)
    {
        var isDeleted = comment.DeletedAt is not null;

        return new CommentDto(
            Id: comment.Id,
            UserId: comment.UserId,
            Username: comment.User?.Username,
            AvatarUrl: comment.User?.AvatarUrl,
            Role: comment.User?.Role.ToString(),
            Content: isDeleted ? null : comment.Content,
            IsDeleted: isDeleted,
            DeletedReason: isDeleted ? comment.DeletedReason : null,
            CreatedAt: comment.CreatedAt
        );
    }
}
