using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.DeleteOwnComment;

public class DeleteOwnCommentCommandHandler
    : IRequestHandler<DeleteOwnCommentCommand, Unit>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;

    public DeleteOwnCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository
    )
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
    }

    public async Task<Unit> Handle(
        DeleteOwnCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var comment = await _commentRepository.GetByIdAsync(
            request.CommentId,
            cancellationToken
        );

        if (comment is null)
        {
            throw new NotFoundAppException("Komentar tidak ditemukan.");
        }

        if (comment.DeletedAt is not null)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["comment"] = new[] { "Komentar sudah dihapus." }
                }
            );
        }

        if (comment.UserId != request.UserId)
        {
            throw new ForbiddenAccessAppException(
                "Kamu hanya bisa menghapus komentar milikmu sendiri."
            );
        }

        await _commentRepository.SoftDeleteAsync(
            request.CommentId,
            request.UserId,
            "Dihapus oleh pemilik komentar.",
            cancellationToken
        );

        var post = await _postRepository.GetByIdAsync(
            comment.PostId,
            cancellationToken
        );

        if (post is not null)
        {
            post.CommentCount = Math.Max(0, post.CommentCount - 1);
            await _postRepository.UpdateAsync(post, cancellationToken);

            var commentCount = await _commentRepository.GetCommentCountAsync(
                comment.PostId,
                cancellationToken
            );

            post.CommentCount = commentCount;
            await _postRepository.UpdateAsync(post, cancellationToken);
        }

        return Unit.Value;
    }
}
