using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Commands.MarkStoryViewed;

public class MarkStoryViewedCommandHandler
    : IRequestHandler<MarkStoryViewedCommand, Unit>
{
    private readonly IStoryRepository _storyRepository;

    public MarkStoryViewedCommandHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    public async Task<Unit> Handle(
        MarkStoryViewedCommand request,
        CancellationToken cancellationToken
    )
    {
        var story = await _storyRepository.GetByIdAsync(
            request.StoryId,
            cancellationToken
        );

        StoryAccessRules.EnsureStoryExists(story);
        StoryAccessRules.EnsureStoryIsActive(story!);

        await _storyRepository.MarkAsViewedAsync(
            request.StoryId,
            request.AccountId,
            cancellationToken
        );

        return Unit.Value;
    }
}
