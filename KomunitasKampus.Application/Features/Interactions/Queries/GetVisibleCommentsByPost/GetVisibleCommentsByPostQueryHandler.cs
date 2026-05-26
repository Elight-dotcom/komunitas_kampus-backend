using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetCommentsByPost;

public class GetVisibleCommentsByPostQueryHandler
    : IRequestHandler<GetVisibleCommentsByPostQuery, IReadOnlyList<CommentDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public GetVisibleCommentsByPostQueryHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository
    )
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IReadOnlyList<CommentDto>> Handle(
        GetVisibleCommentsByPostQuery request,
        CancellationToken cancellationToken
    )
    {
        var post = await _postRepository.GetByIdAsync(
            request.PostId,
            cancellationToken
        );

        InteractionAccessRules.EnsurePostExists(post);

        var canView = await CanViewPostAsync(
            post!.OrganizationId,
            post.Visibility.ToString(),
            request.ViewerAccountId,
            request.ViewerOrganizationId,
            cancellationToken
        );

        if (!canView)
        {
            throw new ForbiddenAccessAppException(
                "Kamu tidak memiliki akses untuk melihat komentar postingan ini."
            );
        }

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

    private async Task<bool> CanViewPostAsync(
        Guid postOrganizationId,
        string visibility,
        Guid? viewerAccountId,
        Guid? viewerOrganizationId,
        CancellationToken cancellationToken
    )
    {
        if (string.Equals(visibility, "Public", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var isOwnerOrganization = viewerOrganizationId == postOrganizationId;

        if (string.Equals(visibility, "Private", StringComparison.OrdinalIgnoreCase))
        {
            return isOwnerOrganization;
        }

        if (string.Equals(visibility, "Internal", StringComparison.OrdinalIgnoreCase))
        {
            if (isOwnerOrganization)
            {
                return true;
            }

            if (!viewerAccountId.HasValue)
            {
                return false;
            }

            return await _postRepository.IsAcceptedMemberAsync(
                postOrganizationId,
                viewerAccountId.Value,
                cancellationToken
            );
        }

        return false;
    }
}
