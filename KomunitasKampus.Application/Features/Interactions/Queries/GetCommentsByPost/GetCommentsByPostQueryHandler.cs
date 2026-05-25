using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryHandler
    : IRequestHandler<GetCommentsByPostQuery, IReadOnlyList<CommentDto>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsByPostQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IReadOnlyList<CommentDto>> Handle(
        GetCommentsByPostQuery request,
        CancellationToken cancellationToken
    )
    {
        var comments = await _commentRepository.GetByPostIdAsync(
            request.PostId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return comments
            .OrderBy(comment => comment.CreatedAt)
            .Select(comment => comment.ToCommentDto())
            .ToList();
    }
}
