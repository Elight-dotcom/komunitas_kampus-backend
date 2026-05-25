using System.Security.Claims;
using KomunitasKampus.API.Contracts.Interactions;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Interactions.Commands.CreateComment;
using KomunitasKampus.Application.Features.Interactions.Commands.DeleteOwnComment;
using KomunitasKampus.Application.Features.Interactions.Commands.ModerateComment;
using KomunitasKampus.Application.Features.Interactions.Commands.SharePost;
using KomunitasKampus.Application.Features.Interactions.Commands.ToggleLike;
using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Application.Features.Interactions.Queries.GetCommentsByPost;
using KomunitasKampus.Application.Features.Interactions.Queries.GetLikeStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[Route("api/posts")]
public class InteractionController : ControllerBase
{
    private const string OrganizationRoles = "organization,organisasi";

    private readonly IMediator _mediator;

    public InteractionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // =========================
    // Like
    // =========================

    [Authorize]
    [HttpPost("{postId:guid}/like")]
    [ProducesResponseType(typeof(ApiResponse<LikeStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleLike(
        Guid postId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new ToggleLikeCommand(
                UserId: GetRequiredAccountId(),
                PostId: postId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<LikeStatusDto>.Ok(
            result,
            result.IsLiked ? "Post berhasil disukai." : "Like berhasil dibatalkan."
        ));
    }

    [Authorize]
    [HttpGet("{postId:guid}/like-status")]
    [ProducesResponseType(typeof(ApiResponse<LikeStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLikeStatus(
        Guid postId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new GetLikeStatusQuery(
                UserId: GetRequiredAccountId(),
                PostId: postId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<LikeStatusDto>.Ok(
            result,
            "Status like berhasil diambil."
        ));
    }

    // =========================
    // Comment
    // =========================

    [AllowAnonymous]
    [HttpGet("{postId:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetComments(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mediator.Send(
            new GetVisibleCommentsByPostQuery(
                PostId: postId,
                ViewerAccountId: GetOptionalAccountId(),
                ViewerOrganizationId: GetOrganizationIdFromClaims(),
                ViewerRole: GetOptionalRole(),
                Page: page,
                PageSize: pageSize
            ),
            cancellationToken
        );

        return Ok(new ApiResponse<IReadOnlyList<CommentDto>>(true, "Komentar berhasil diambil.", result, null));
    }

    [Authorize]
    [HttpPost("{postId:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await _mediator.Send(
                new CreateCommentCommand(
                    UserId: GetRequiredAccountId(),
                    PostId: postId,
                    Content: request.Content
                ),
                cancellationToken
            );

            return Ok(ApiResponse<CommentDto>.Ok(
                result,
                "Komentar berhasil dikirim."
            ));
        }
        catch (ValidationAppException exception) when (IsCommentRateLimitError(exception))
        {
            const int retryAfterSeconds = 60;

            Response.Headers.RetryAfter = retryAfterSeconds.ToString();

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                ApiResponse<object>.Fail(
                    $"Terlalu banyak komentar. Coba lagi dalam {retryAfterSeconds} detik.",
                    exception.Errors
                )
            );
        }
    }

    [Authorize]
    [HttpDelete("{postId:guid}/comments/{commentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteOwnComment(
        Guid postId,
        Guid commentId,
        CancellationToken cancellationToken
    )
    {
        await _mediator.Send(
            new DeleteOwnCommentCommand(
                UserId: GetRequiredAccountId(),
                CommentId: commentId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<object>.Ok(
            new
            {
                postId,
                commentId
            },
            "Komentar berhasil dihapus."
        ));
    }

    // =========================
    // Share
    // =========================

    [Authorize]
    [HttpPost("{postId:guid}/share")]
    [ProducesResponseType(typeof(ApiResponse<ShareResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SharePost(
        Guid postId,
        [FromBody] SharePostRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new SharePostCommand(
                UserId: GetRequiredAccountId(),
                PostId: postId,
                Platform: request.Platform
            ),
            cancellationToken
        );

        return Ok(ApiResponse<ShareResultDto>.Ok(
            result,
            result.RequiresAuth
                ? "Share berhasil dicatat. Link ini hanya bisa dibuka oleh anggota organisasi."
                : "Share berhasil dicatat."
        ));
    }

    // =========================
    // Moderasi Admin
    // =========================

    [Authorize(Roles = OrganizationRoles)]
    [HttpDelete("{postId:guid}/comments/{commentId:guid}/moderate")]
    [ProducesResponseType(typeof(ApiResponse<ModerateCommentResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ModerateComment(
        Guid postId,
        Guid commentId,
        [FromBody] ModerateCommentRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new ModerateCommentCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterOrganizationId: GetOrganizationIdFromClaims(),
                PostId: postId,
                CommentId: commentId,
                DeletedReason: request.DeletedReason
            ),
            cancellationToken
        );

        return Ok(ApiResponse<ModerateCommentResultDto>.Ok(
            result,
            "Komentar berhasil dimoderasi."
        ));
    }

    private Guid GetRequiredAccountId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("account_id") ??
            User.FindFirstValue("accountId");

        if (Guid.TryParse(value, out var accountId))
        {
            return accountId;
        }

        throw new UnauthorizedAccessException("Account id tidak ditemukan di token.");
    }

    private Guid? GetOptionalAccountId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("account_id") ??
            User.FindFirstValue("accountId");

        return Guid.TryParse(value, out var accountId)
            ? accountId
            : null;
    }

    private Guid? GetOrganizationIdFromClaims()
    {
        var value =
            User.FindFirstValue("organization_id") ??
            User.FindFirstValue("organizationId");

        return Guid.TryParse(value, out var organizationId)
            ? organizationId
            : null;
    }

    private string? GetOptionalRole()
    {
        return User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role");
    }

    private static bool IsCommentRateLimitError(ValidationAppException exception)
    {
        return exception.Errors.Values
            .SelectMany(messages => messages)
            .Any(message =>
                message.Contains("5 komentar", StringComparison.OrdinalIgnoreCase)
                || message.Contains("terlalu sering", StringComparison.OrdinalIgnoreCase)
                || message.Contains("spam", StringComparison.OrdinalIgnoreCase)
            );
    }
}
