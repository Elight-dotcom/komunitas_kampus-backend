using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Domain.Enums;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Commands.CreateStory;

public sealed record CreateStoryCommand(
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    Guid OrganizationId,
    StoryMediaType MediaType,
    string? FileKey,
    string? TextContent
) : IRequest<StoryDto>;
