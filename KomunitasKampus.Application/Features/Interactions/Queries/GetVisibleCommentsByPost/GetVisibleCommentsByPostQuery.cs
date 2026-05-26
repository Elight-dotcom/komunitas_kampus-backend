using KomunitasKampus.Application.Features.Interactions.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetCommentsByPost;

public sealed record GetVisibleCommentsByPostQuery(
    Guid PostId,
    Guid? ViewerAccountId,
    Guid? ViewerOrganizationId,
    string? ViewerRole,
    int Page = 1,
    int PageSize = 20
) : IRequest<IReadOnlyList<CommentDto>>;
