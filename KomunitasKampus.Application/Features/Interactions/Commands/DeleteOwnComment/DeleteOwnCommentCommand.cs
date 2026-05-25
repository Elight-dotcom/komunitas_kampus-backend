using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.DeleteOwnComment;

public sealed record DeleteOwnCommentCommand(
    Guid UserId,
    Guid CommentId
) : IRequest<Unit>;
