using KomunitasKampus.Domain.Enums;
using MediatR;

namespace KomunitasKampus.Application.Features.Stories.Commands.GeneratePresignedUploadUrl;

public sealed record GenerateStoryPresignedUploadUrlCommand(
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    Guid OrganizationId,
    string FileName,
    StoryMediaType MediaType,
    long FileSize
) : IRequest<StoryPresignedUploadUrlDto>;
