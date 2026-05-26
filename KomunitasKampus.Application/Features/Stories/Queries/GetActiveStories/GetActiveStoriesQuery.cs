using KomunitasKampus.Application.Features.Stories.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetActiveStories;

public sealed record GetActiveStoriesQuery(
    Guid? ViewerAccountId
) : IRequest<IReadOnlyList<StoryGroupDto>>;
