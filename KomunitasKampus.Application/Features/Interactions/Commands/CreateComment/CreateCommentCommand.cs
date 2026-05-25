using KomunitasKampus.Application.Features.Interactions.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.CreateComment;

public sealed record CreateCommentCommand(
    Guid UserId,
    Guid PostId,
    string Content
) : IRequest<CommentDto>;
