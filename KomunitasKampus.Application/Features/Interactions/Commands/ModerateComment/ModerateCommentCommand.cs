using KomunitasKampus.Application.Features.Interactions.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.ModerateComment;

public sealed record ModerateCommentCommand(
    Guid RequesterAccountId,
    Guid? RequesterOrganizationId,
    Guid PostId,
    Guid CommentId,
    string DeletedReason
) : IRequest<ModerateCommentResultDto>;
