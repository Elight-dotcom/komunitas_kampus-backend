using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.GeneratePresignedUploadUrl;

public class GeneratePresignedUploadUrlCommandHandler
    : IRequestHandler<GeneratePresignedUploadUrlCommand, PresignedUploadUrlResponse>
{
    private readonly IPostRepository _postRepository;

    public GeneratePresignedUploadUrlCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PresignedUploadUrlResponse> Handle(
        GeneratePresignedUploadUrlCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!IsOrganizationRole(request.RequesterRole))
        {
            throw new ForbiddenAccessAppException("Hanya akun organisasi yang boleh mengunggah file.");
        }

        if (request.RequesterOrganizationId != request.OrganizationId)
        {
            throw new ForbiddenAccessAppException("Akun tidak boleh mengakses organisasi lain.");
        }

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var fileKey = $"posts/{request.OrganizationId}/{Guid.NewGuid():N}{extension}";

        var uploadUrl = await _postRepository.GetPresignedUploadUrlAsync(
            fileKey,
            request.MediaType,
            cancellationToken
        );

        return new PresignedUploadUrlResponse(
            UploadUrl: uploadUrl,
            FileKey: fileKey,
            MediaType: request.MediaType,
            FileSize: request.FileSize,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15)
        );
    }

    private static bool IsOrganizationRole(string role)
    {
        return string.Equals(role, "organization", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "organisasi", StringComparison.OrdinalIgnoreCase);
    }
}
