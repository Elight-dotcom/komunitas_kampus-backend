using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetActiveStories;

public class GetActiveStoriesQueryHandler
    : IRequestHandler<GetActiveStoriesQuery, IReadOnlyList<StoryGroupDto>>
{
    private readonly IStoryRepository _storyRepository;

    public GetActiveStoriesQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    public async Task<IReadOnlyList<StoryGroupDto>> Handle(
        GetActiveStoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var stories = await _storyRepository.GetActiveStoriesAsync(
            request.ViewerAccountId,
            cancellationToken
        );

        return stories
            .Where(story => !story.IsExpired && story.ExpiresAt > DateTime.UtcNow)
            .GroupBy(story => story.OrganizationId)
            .Select(group => StoryMappingExtensions.ToStoryGroupDto(
                group.Key,
                group.ToList(),
                request.ViewerAccountId
            ))
            .OrderByDescending(group => group.HasUnviewed)
            .ThenBy(group => group.OrgName)
            .ToList();
    }
}
