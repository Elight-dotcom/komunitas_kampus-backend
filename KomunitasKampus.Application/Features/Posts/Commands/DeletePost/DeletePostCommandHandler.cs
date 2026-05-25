using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, DeletePostResponse>
{
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<DeletePostResponse> Handle(
        DeletePostCommand request,
        CancellationToken cancellationToken
    )
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            throw new NotFoundAppException("Postingan tidak ditemukan.");
        }

        if (!PostAccessRules.CanManageOrganization(
            request.RequesterRole,
            request.RequesterOrganizationId,
            post.OrganizationId
        ))
        {
            throw new ForbiddenAccessAppException("Hanya organisasi pemilik post yang boleh menghapus postingan.");
        }

        await _postRepository.DeleteAsync(post.Id, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        return new DeletePostResponse(
            PostId: post.Id,
            Message: "Postingan berhasil dihapus. File media terkait perlu dihapus oleh implementasi Infrastructure/MinIO."
        );
    }
}
