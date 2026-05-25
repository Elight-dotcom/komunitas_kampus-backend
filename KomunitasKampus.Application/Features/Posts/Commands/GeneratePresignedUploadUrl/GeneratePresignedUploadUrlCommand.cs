using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Enums;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.GeneratePresignedUploadUrl;

public sealed record GeneratePresignedUploadUrlCommand(
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    Guid OrganizationId,
    string FileName,
    PostMediaType MediaType,
    int FileSize
) : IRequest<PresignedUploadUrlResponse>;
