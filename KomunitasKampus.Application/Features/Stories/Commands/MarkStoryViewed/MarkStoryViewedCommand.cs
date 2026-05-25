using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Commands.MarkStoryViewed;

public sealed record MarkStoryViewedCommand(
    Guid StoryId,
    Guid AccountId
) : IRequest<Unit>;
