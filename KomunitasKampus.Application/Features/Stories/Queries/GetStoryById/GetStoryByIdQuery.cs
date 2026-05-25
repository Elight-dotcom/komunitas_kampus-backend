using KomunitasKampus.Application.Features.Stories.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetStoryById;

public sealed record GetStoryByIdQuery(
    Guid StoryId,
    Guid? ViewerAccountId
) : IRequest<StoryDto>;
