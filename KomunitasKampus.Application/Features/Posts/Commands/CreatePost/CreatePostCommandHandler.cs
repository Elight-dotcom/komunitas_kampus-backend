using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, CreatePostResponse>
{
    private readonly IPostRepository _postRepository;

    public CreatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<CreatePostResponse> Handle(
        CreatePostCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!PostAccessRules.CanManageOrganization(
            request.RequesterRole,
            request.RequesterOrganizationId,
            request.RequesterOrganizationId ?? Guid.Empty
        ))
        {
            throw new ForbiddenAccessAppException("Hanya akun organisasi yang boleh membuat postingan.");
        }

        var organizationId = request.RequesterOrganizationId!.Value;

        var visibility = Enum.Parse<PostVisibility>(request.Payload.Visibility, ignoreCase: true);

        var post = new Post
        {
            OrganizationId = organizationId,
            Title = request.Payload.Title.Trim(),
            Caption = string.IsNullOrWhiteSpace(request.Payload.Caption)
                ? null
                : request.Payload.Caption.Trim(),
            IsPinned = request.Payload.IsPinned,
            PinOrder = request.Payload.IsPinned ? request.Payload.PinOrder : null,
            Visibility = visibility,
            LikeCount = 0,
            CommentCount = 0,
            ShareCount = 0
        };

        var uploads = new List<PresignedUploadDto>();

        foreach (var mediaRequest in request.Payload.Media.OrderBy(media => media.OrderIndex))
        {
            var mediaType = Enum.Parse<PostMediaType>(mediaRequest.MediaType, ignoreCase: true);

            var uploadUrl = await _postRepository.GetPresignedUploadUrlAsync(
                mediaRequest.FileName,
                mediaType,
                cancellationToken
            );

            post.Media.Add(new PostMedia
            {
                MediaType = mediaType,
                FileUrl = mediaRequest.FileName,
                FileSizeBytes = mediaRequest.FileSizeBytes,
                OrderIndex = mediaRequest.OrderIndex,
                Status = PostMediaStatus.Ready // File sudah ter-upload ke MinIO via presigned URL
            });

            uploads.Add(new PresignedUploadDto(
                FileName: mediaRequest.FileName,
                MediaType: mediaType.ToString(),
                OrderIndex: mediaRequest.OrderIndex,
                UploadUrl: uploadUrl
            ));
        }

        await _postRepository.CreateAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        return new CreatePostResponse(
            Post: post.ToDto(),
            Uploads: uploads
        );
    }
}
