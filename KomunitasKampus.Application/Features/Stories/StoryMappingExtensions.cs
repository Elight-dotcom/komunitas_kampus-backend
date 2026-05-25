using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Application.Features.Stories;

public static class StoryMappingExtensions
{
    public static StoryDto ToStoryDto(
        this Story story,
        Guid? viewerAccountId
    )
    {
        var isViewed = viewerAccountId.HasValue &&
            story.Views.Any(view => view.AccountId == viewerAccountId.Value);

        return new StoryDto(
            Id: story.Id,
            OrganizationId: story.OrganizationId,
            OrgName: story.Organization?.OrganizationName ?? string.Empty,
            OrgAvatar: story.Organization?.Account?.AvatarUrl,
            MediaType: story.MediaType,
            MediaUrl: story.MediaUrl,
            TextContent: story.TextContent,
            ExpiresAt: story.ExpiresAt,
            IsViewed: isViewed
        );
    }

    public static StoryGroupDto ToStoryGroupDto(
        Guid organizationId,
        IReadOnlyList<Story> stories,
        Guid? viewerAccountId
    )
    {
        var firstStory = stories.First();

        var storyDtos = stories
            .OrderBy(story => story.CreatedAt)
            .Select(story => story.ToStoryDto(viewerAccountId))
            .ToList();

        return new StoryGroupDto(
            OrganizationId: organizationId,
            OrgName: firstStory.Organization?.OrganizationName ?? string.Empty,
            OrgAvatar: firstStory.Organization?.Account?.AvatarUrl,
            HasUnviewed: storyDtos.Any(story => !story.IsViewed),
            Stories: storyDtos
        );
    }
}
