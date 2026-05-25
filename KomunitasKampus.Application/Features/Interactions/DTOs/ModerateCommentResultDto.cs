namespace KomunitasKampus.Application.Features.Interactions.DTOs;

public sealed record ModerateCommentResultDto(
    Guid PostId,
    Guid CommentId,
    string DeletedReason,
    int CommentCount
);
