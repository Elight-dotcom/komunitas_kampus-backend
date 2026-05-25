using KomunitasKampus.Application.Features.Interactions.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetCommentsByPost;

public sealed record GetCommentsByPostQuery(
    Guid PostId,
    int Page = 1,
    int PageSize = 20
) : IRequest<IReadOnlyList<CommentDto>>;
