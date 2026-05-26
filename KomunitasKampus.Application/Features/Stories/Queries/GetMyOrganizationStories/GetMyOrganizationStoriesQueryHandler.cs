using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetMyOrganizationStories;

public class GetMyOrganizationStoriesQueryHandler
    : IRequestHandler<GetMyOrganizationStoriesQuery, IReadOnlyList<StoryDto>>
{
    private readonly IStoryRepository _storyRepository;

    public GetMyOrganizationStoriesQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    public async Task<IReadOnlyList<StoryDto>> Handle(
        GetMyOrganizationStoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var stories = await _storyRepository.GetActiveStoriesByOrganizationAsync(
            request.OrganizationId,
            cancellationToken
        );

        return stories
            .Where(story => !story.IsExpired && story.ExpiresAt > DateTime.UtcNow)
            .OrderBy(story => story.CreatedAt)
            .Select(story => story.ToStoryDto(null))
            .ToList();
    }
}