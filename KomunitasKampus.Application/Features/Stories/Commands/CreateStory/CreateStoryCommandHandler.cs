using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Commands.CreateStory;

public class CreateStoryCommandHandler
    : IRequestHandler<CreateStoryCommand, StoryDto>
{
    private const int MaxActiveStoriesPerOrganization = 10;

    private readonly IStoryRepository _storyRepository;
    private readonly IStoryBackgroundJobScheduler _backgroundJobScheduler;

    public CreateStoryCommandHandler(
        IStoryRepository storyRepository,
        IStoryBackgroundJobScheduler backgroundJobScheduler
    )
    {
        _storyRepository = storyRepository;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task<StoryDto> Handle(
        CreateStoryCommand request,
        CancellationToken cancellationToken
    )
    {
        StoryAccessRules.EnsureOrganizationRole(request.RequesterRole);
        StoryAccessRules.EnsureOrganizationOwnsStoryRequest(
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

        var now = DateTime.UtcNow;

        var story = new Story
        {
            OrganizationId = request.OrganizationId,
            MediaType = request.MediaType,
            MediaUrl = request.MediaType == StoryMediaType.Text
                ? null
                : request.FileKey?.Trim(),
            TextContent = request.MediaType == StoryMediaType.Text
                ? request.TextContent?.Trim()
                : request.TextContent?.Trim(),
            IsExpired = false,
            CreatedAt = now,
            UpdatedAt = now,
            ExpiresAt = now.AddHours(24)
        };

        await _storyRepository.CreateAsync(story, cancellationToken);

        if (story.MediaType == StoryMediaType.Video && !string.IsNullOrWhiteSpace(story.MediaUrl))
        {
            await _backgroundJobScheduler.EnqueueStoryVideoTranscodeAsync(
                story.Id,
                story.MediaUrl,
                cancellationToken
            );
        }

        return story.ToStoryDto(request.RequesterAccountId);
    }
}
