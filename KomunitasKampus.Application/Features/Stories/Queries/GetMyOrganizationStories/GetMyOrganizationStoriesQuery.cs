using KomunitasKampus.Application.Features.Stories.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetMyOrganizationStories;

public sealed record GetMyOrganizationStoriesQuery(
    Guid OrganizationId
) : IRequest<IReadOnlyList<StoryDto>>;