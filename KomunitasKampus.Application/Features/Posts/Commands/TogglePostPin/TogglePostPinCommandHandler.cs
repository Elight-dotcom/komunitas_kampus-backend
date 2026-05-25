using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.TogglePostPin;

public class TogglePostPinCommandHandler
    : IRequestHandler<TogglePostPinCommand, TogglePostPinResponse>
{
    private readonly IPostRepository _postRepository;

    public TogglePostPinCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<TogglePostPinResponse> Handle(
        TogglePostPinCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!IsOrganizationRole(request.RequesterRole))
        {
            throw new ForbiddenAccessAppException("Hanya akun organisasi yang boleh mengubah pin post.");
        }

        if (request.RequesterOrganizationId != request.OrganizationId)
        {
            throw new ForbiddenAccessAppException("Akun tidak boleh mengakses organisasi lain.");
        }

        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            throw new NotFoundAppException("Post tidak ditemukan.");
        }

        if (post.OrganizationId != request.OrganizationId)
        {
            throw new ForbiddenAccessAppException("Post bukan milik organisasi ini.");
        }

        if (request.IsPinned && !post.IsPinned)
        {
            var pinnedCount = await _postRepository.CountPinnedPostsAsync(
                request.OrganizationId,
                excludedPostId: request.PostId,
                cancellationToken
            );

            if (pinnedCount >= 3)
            {
                throw new ValidationAppException(
                    new Dictionary<string, string[]>
                    {
                        ["isPinned"] =
                        [
                            "Maksimal hanya 3 post yang boleh di-pin dalam satu organisasi."
                        ]
                    }
                );
            }
        }

        post.IsPinned = request.IsPinned;
        post.PinOrder = request.IsPinned ? request.PinOrder : null;

        await _postRepository.UpdateAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        return new TogglePostPinResponse(
            PostId: post.Id,
            IsPinned: post.IsPinned,
            PinOrder: post.PinOrder
        );
    }

    private static bool IsOrganizationRole(string role)
    {
        return string.Equals(role, "organization", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "organisasi", StringComparison.OrdinalIgnoreCase);
    }
}
