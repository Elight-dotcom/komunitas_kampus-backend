using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Commands.GeneratePresignedUploadUrl;

public class GenerateStoryPresignedUploadUrlCommandHandler
    : IRequestHandler<GenerateStoryPresignedUploadUrlCommand, StoryPresignedUploadUrlDto>
{
    private const int MaxActiveStoriesPerOrganization = 10;

    private readonly IStoryRepository _storyRepository;
    private readonly IStoryUploadService _storyUploadService;

    public GenerateStoryPresignedUploadUrlCommandHandler(
        IStoryRepository storyRepository,
        IStoryUploadService storyUploadService
    )
    {
        _storyRepository = storyRepository;
        _storyUploadService = storyUploadService;
    }

    public async Task<StoryPresignedUploadUrlDto> Handle(
        GenerateStoryPresignedUploadUrlCommand request,
        CancellationToken cancellationToken
    )
    {
        EnsureOrganizationRole(request.RequesterRole);
        EnsureOrganizationMatchesRequest(
            request.OrganizationId,
            request.RequesterOrganizationId
        );

        var activeStoryCount = await _storyRepository.GetActiveCountByOrgAsync(
            request.OrganizationId,
            cancellationToken
        );

        if (activeStoryCount >= MaxActiveStoriesPerOrganization)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["story"] = new[]
                    {
                        "Organisasi sudah mencapai batas maksimal 10 story aktif."
                    }
                }
            );
        }

        var extension = Path
            .GetExtension(request.FileName)
            .ToLowerInvariant();

        var contentType = ResolveContentType(request.MediaType, extension);
        var fileKey = BuildStoryFileKey(
            request.OrganizationId,
            request.MediaType,
            extension
        );

        var uploadUrl = await _storyUploadService.GeneratePresignedPutUrlAsync(
            fileKey,
            contentType,
            cancellationToken
        );

        return new StoryPresignedUploadUrlDto(
            UploadUrl: uploadUrl,
            FileKey: fileKey,
            MediaType: request.MediaType,
            FileSize: request.FileSize,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15)
        );
    }

    private static void EnsureOrganizationRole(string requesterRole)
    {
        var normalizedRole = requesterRole.Trim().ToLowerInvariant();

        if (normalizedRole is not ("organization" or "organisasi"))
        {
            throw new ForbiddenAccessAppException(
                "Hanya akun organisasi yang boleh membuat presigned URL story."
            );
        }
    }

    private static void EnsureOrganizationMatchesRequest(
        Guid organizationId,
        Guid? requesterOrganizationId
    )
    {
        if (!requesterOrganizationId.HasValue ||
            requesterOrganizationId.Value != organizationId)
        {
            throw new ForbiddenAccessAppException(
                "Akun organisasi hanya boleh membuat presigned URL untuk organisasinya sendiri."
            );
        }
    }

    private static string ResolveContentType(
        StoryMediaType mediaType,
        string extension
    )
    {
        return mediaType switch
        {
            StoryMediaType.Image when extension == ".jpg" || extension == ".jpeg" => "image/jpeg",
            StoryMediaType.Image when extension == ".png" => "image/png",
            StoryMediaType.Video when extension == ".mp4" => "video/mp4",
            _ => throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["fileName"] = new[] { "Ekstensi file tidak sesuai dengan media type." }
                }
            )
        };
    }

    private static string BuildStoryFileKey(
        Guid organizationId,
        StoryMediaType mediaType,
        string extension
    )
    {
        var folder = mediaType == StoryMediaType.Video
            ? "videos"
            : "images";

        return $"stories/{organizationId:N}/{folder}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
    }
}
