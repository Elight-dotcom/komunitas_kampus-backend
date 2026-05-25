using System.Security.Claims;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Features.Stories.Commands.CreateStory;
using KomunitasKampus.Application.Features.Stories.Commands.GeneratePresignedUploadUrl;
using KomunitasKampus.Application.Features.Stories.Commands.MarkStoryViewed;
using KomunitasKampus.Application.Features.Stories.DTOs;
using KomunitasKampus.Application.Features.Stories.Queries.GetActiveStories;
using KomunitasKampus.Application.Features.Stories.Queries.GetStoryById;
using KomunitasKampus.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
public class StoriesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StoriesController> _logger;

    public StoriesController(
        ISender sender,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<StoriesController> logger
    )
    {
        _sender = sender;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    [HttpGet("api/stories")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StoryGroupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveStories(
        CancellationToken cancellationToken
    )
    {
        var viewerAccountId = GetRequiredAccountId();

        var result = await _sender.Send(
            new GetActiveStoriesQuery(viewerAccountId),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<StoryGroupDto>>.Ok(
            result,
            "Stories aktif berhasil diambil."
        ));
    }

    [HttpGet("api/stories/{storyId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<StoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStoryById(
        Guid storyId,
        CancellationToken cancellationToken
    )
    {
        var viewerAccountId = GetRequiredAccountId();

        var result = await _sender.Send(
            new GetStoryByIdQuery(storyId, viewerAccountId),
            cancellationToken
        );

        return Ok(ApiResponse<StoryDto>.Ok(
            result,
            "Story berhasil diambil."
        ));
    }

    [HttpPost("api/organizations/{orgId:guid}/stories/presigned-url")]
    [Authorize(Roles = "organization")]
    [ProducesResponseType(typeof(ApiResponse<StoryPresignedUploadUrlDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GeneratePresignedUploadUrl(
        Guid orgId,
        [FromBody] GenerateStoryPresignedUploadUrlRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!IsRequesterOrganization(orgId))
        {
            return Forbid();
        }

        var result = await _sender.Send(
            new GenerateStoryPresignedUploadUrlCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationId(),
                OrganizationId: orgId,
                FileName: request.FileName,
                MediaType: request.MediaType,
                FileSize: request.FileSize
            ),
            cancellationToken
        );

        return Ok(ApiResponse<StoryPresignedUploadUrlDto>.Ok(
            result,
            "Presigned URL story berhasil dibuat."
        ));
    }

    [HttpPost("api/organizations/{orgId:guid}/stories")]
    [Authorize(Roles = "organization")]
    [ProducesResponseType(typeof(ApiResponse<StoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateStory(
        Guid orgId,
        [FromBody] CreateStoryRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!IsRequesterOrganization(orgId))
        {
            return Forbid();
        }

        var result = await _sender.Send(
            new CreateStoryCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationId(),
                OrganizationId: orgId,
                MediaType: request.MediaType,
                FileKey: request.FileKey,
                TextContent: request.TextContent
            ),
            cancellationToken
        );

        return Ok(ApiResponse<StoryDto>.Ok(
            result,
            "Story berhasil dibuat."
        ));
    }

    [HttpPost("api/stories/{storyId:guid}/view")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MarkStoryViewedResponse>), StatusCodes.Status200OK)]
    public IActionResult MarkStoryViewed(
        Guid storyId
    )
    {
        var accountId = GetRequiredAccountId();

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                await sender.Send(
                    new MarkStoryViewedCommand(storyId, accountId)
                );
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Gagal menjalankan fire-and-forget MarkStoryViewed untuk Story {StoryId} dan Account {AccountId}.",
                    storyId,
                    accountId
                );
            }
        });

        return Ok(ApiResponse<MarkStoryViewedResponse>.Ok(
            new MarkStoryViewedResponse(
                StoryId: storyId,
                Message: "Story view sedang diproses."
            ),
            "Story view diterima."
        ));
    }

    private Guid GetRequiredAccountId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("account_id")
            ?? User.FindFirstValue("accountId")
            ?? User.FindFirstValue("id");

        return Guid.TryParse(value, out var accountId)
            ? accountId
            : throw new UnauthorizedAccessException("Claim account id tidak ditemukan.");
    }

    private Guid? GetOrganizationId()
    {
        var value = User.FindFirstValue("organization_id")
            ?? User.FindFirstValue("organizationId")
            ?? User.FindFirstValue("org_id")
            ?? User.FindFirstValue("orgId");

        return Guid.TryParse(value, out var organizationId)
            ? organizationId
            : null;
    }

    private string GetRequiredRole()
    {
        return User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role")
            ?? throw new UnauthorizedAccessException("Claim role tidak ditemukan.");
    }

    private bool IsRequesterOrganization(Guid organizationId)
    {
        var requesterOrganizationId = GetOrganizationId();

        return requesterOrganizationId.HasValue &&
            requesterOrganizationId.Value == organizationId;
    }
}

public sealed record GenerateStoryPresignedUploadUrlRequest(
    string FileName,
    StoryMediaType MediaType,
    long FileSize
);

public sealed record CreateStoryRequest(
    StoryMediaType MediaType,
    string? FileKey,
    string? TextContent,
    string? BackgroundColor
);

public sealed record MarkStoryViewedResponse(
    Guid StoryId,
    string Message
);
