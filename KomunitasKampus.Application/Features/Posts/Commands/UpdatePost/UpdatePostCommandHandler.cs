using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IPostRepository _postRepository;

    public UpdatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto> Handle(
        UpdatePostCommand request,
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
            throw new ForbiddenAccessAppException("Hanya organisasi pemilik post yang boleh mengedit postingan.");
        }

        post.Title = request.Payload.Title.Trim();
        post.Caption = string.IsNullOrWhiteSpace(request.Payload.Caption)
            ? null
            : request.Payload.Caption.Trim();

        await _postRepository.UpdateAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        return post.ToDto();
    }
}
