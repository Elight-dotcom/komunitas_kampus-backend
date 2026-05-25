using System.Security.Claims;
using KomunitasKampus.API.Contracts.Posts;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.Commands.CreatePost;
using KomunitasKampus.Application.Features.Posts.Commands.DeletePost;
using KomunitasKampus.Application.Features.Posts.Commands.GeneratePresignedUploadUrl;
using KomunitasKampus.Application.Features.Posts.Commands.TogglePostPin;
using KomunitasKampus.Application.Features.Posts.Commands.UpdatePost;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Application.Features.Posts.Queries.GetFeed;
using KomunitasKampus.Application.Features.Posts.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[Route("api/organizations/{orgId:guid}/posts")]
public class PostsController : ControllerBase
{
    private const string OrganizationRolePolicy = "organization,Organisasi";

    private readonly ISender _sender;

    public PostsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PostDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeed(
        [FromRoute] Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var query = new GetFeedQuery(
            OrganizationId: orgId,
            ViewerAccountId: GetAccountIdOrNull(),
            ViewerRole: GetRoleOrNull(),
            ViewerOrganizationId: GetOrganizationIdOrNull(),
            Page: page,
            PageSize: pageSize
        );

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<PostDto>>.Ok(
            result,
            "Feed berhasil diambil."
        ));
    }

    [HttpGet("{postId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(
        [FromRoute] Guid orgId,
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = new GetPostByIdQuery(
                PostId: postId,
                ViewerAccountId: GetAccountIdOrNull(),
                ViewerRole: GetRoleOrNull(),
                ViewerOrganizationId: GetOrganizationIdOrNull()
            );

            var result = await _sender.Send(query, cancellationToken);

            return Ok(ApiResponse<PostDto>.Ok(
                result,
                "Post berhasil diambil."
            ));
        }
        catch (ForbiddenAccessAppException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<PostDto>.Fail(exception.Message)
            );
        }
        catch (NotFoundAppException exception)
        {
            return NotFound(ApiResponse<PostDto>.Fail(exception.Message));
        }
    }

    [HttpPost("presigned-url")]
    [Authorize(Roles = OrganizationRolePolicy)]
    [ProducesResponseType(typeof(ApiResponse<PresignedUploadUrlResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PresignedUploadUrlResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PresignedUploadUrlResponse>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GeneratePresignedUploadUrl(
        [FromRoute] Guid orgId,
        [FromBody] PresignedUploadUrlRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var authorizationError = ValidateOrganizationAccess(orgId);

            if (authorizationError is not null)
            {
                return authorizationError;
            }

            var command = new GeneratePresignedUploadUrlCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationIdOrNull(),
                OrganizationId: orgId,
                FileName: request.FileName,
                MediaType: request.MediaType,
                FileSize: request.FileSize
            );

            var result = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<PresignedUploadUrlResponse>.Ok(
                result,
                "Presigned upload URL berhasil dibuat."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<PresignedUploadUrlResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
        catch (ForbiddenAccessAppException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<PresignedUploadUrlResponse>.Fail(exception.Message)
            );
        }
    }

    [HttpPost]
    [Authorize(Roles = OrganizationRolePolicy)]
    [ProducesResponseType(typeof(ApiResponse<CreatePostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreatePostResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreatePostResponse>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePost(
        [FromRoute] Guid orgId,
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var authorizationError = ValidateOrganizationAccess(orgId);

            if (authorizationError is not null)
            {
                return authorizationError;
            }

            var payload = new CreatePostDto(
                Title: request.Title,
                Caption: request.Caption,
                IsPinned: request.IsPinned,
                PinOrder: request.PinOrder,
                Visibility: request.Visibility.ToString(),
                Media: request.MediaItems
                    .Select(media => new CreatePostMediaDto(
                        FileName: media.FileKey,
                        MediaType: media.MediaType.ToString(),
                        FileSizeBytes: media.FileSize,
                        OrderIndex: media.OrderIndex
                    ))
                    .ToList()
            );

            var command = new CreatePostCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationIdOrNull(),
                Payload: payload
            );

            var result = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<CreatePostResponse>.Ok(
                result,
                "Post berhasil dibuat."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<CreatePostResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
        catch (ForbiddenAccessAppException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<CreatePostResponse>.Fail(exception.Message)
            );
        }
    }

    [HttpPut("{postId:guid}")]
    [Authorize(Roles = OrganizationRolePolicy)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePost(
        [FromRoute] Guid orgId,
        [FromRoute] Guid postId,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var authorizationError = ValidateOrganizationAccess(orgId);

            if (authorizationError is not null)
            {
                return authorizationError;
            }

            var command = new UpdatePostCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationIdOrNull(),
                PostId: postId,
                Payload: new UpdatePostDto(
                    Title: request.Title,
                    Caption: request.Caption
                )
            );

            var result = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<PostDto>.Ok(
                result,
                "Post berhasil diperbarui."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<PostDto>.Fail(exception.Message, exception.Errors));
        }
        catch (ForbiddenAccessAppException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PostDto>.Fail(exception.Message));
        }
        catch (NotFoundAppException exception)
        {
            return NotFound(ApiResponse<PostDto>.Fail(exception.Message));
        }
    }

    [HttpDelete("{postId:guid}")]
    [Authorize(Roles = OrganizationRolePolicy)]
    [ProducesResponseType(typeof(ApiResponse<DeletePostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeletePostResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DeletePostResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(
        [FromRoute] Guid orgId,
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var authorizationError = ValidateOrganizationAccess(orgId);

            if (authorizationError is not null)
            {
                return authorizationError;
            }

            var command = new DeletePostCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationIdOrNull(),
                PostId: postId
            );

            var result = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<DeletePostResponse>.Ok(
                result,
                "Post berhasil dihapus."
            ));
        }
        catch (ForbiddenAccessAppException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<DeletePostResponse>.Fail(exception.Message));
        }
        catch (NotFoundAppException exception)
        {
            return NotFound(ApiResponse<DeletePostResponse>.Fail(exception.Message));
        }
    }

    [HttpPatch("{postId:guid}/pin")]
    [Authorize(Roles = OrganizationRolePolicy)]
    [ProducesResponseType(typeof(ApiResponse<TogglePostPinResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TogglePostPinResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TogglePostPinResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TogglePostPinResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TogglePin(
        [FromRoute] Guid orgId,
        [FromRoute] Guid postId,
        [FromBody] TogglePostPinRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var authorizationError = ValidateOrganizationAccess(orgId);

            if (authorizationError is not null)
            {
                return authorizationError;
            }

            var command = new TogglePostPinCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationIdOrNull(),
                OrganizationId: orgId,
                PostId: postId,
                IsPinned: request.IsPinned,
                PinOrder: request.PinOrder
            );

            var result = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<TogglePostPinResponse>.Ok(
                result,
                "Status pin post berhasil diperbarui."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<TogglePostPinResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
        catch (ForbiddenAccessAppException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<TogglePostPinResponse>.Fail(exception.Message)
            );
        }
        catch (NotFoundAppException exception)
        {
            return NotFound(ApiResponse<TogglePostPinResponse>.Fail(exception.Message));
        }
    }

    private IActionResult? ValidateOrganizationAccess(Guid organizationIdFromRoute)
    {
        var organizationIdFromToken = GetOrganizationIdOrNull();

        if (organizationIdFromToken is null)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail("Organization id tidak ditemukan pada token.")
            );
        }

        if (organizationIdFromToken != organizationIdFromRoute)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail("Akun tidak boleh mengakses post organisasi lain.")
            );
        }

        return null;
    }

    private Guid? GetAccountIdOrNull()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("account_id");

        return Guid.TryParse(value, out var accountId)
            ? accountId
            : null;
    }

    private Guid GetRequiredAccountId()
    {
        return GetAccountIdOrNull()
            ?? throw new UnauthorizedAccessException("Account id tidak ditemukan pada token.");
    }

    private Guid? GetOrganizationIdOrNull()
    {
        var value =
            User.FindFirstValue("organization_id") ??
            User.FindFirstValue("organizationId");

        return Guid.TryParse(value, out var organizationId)
            ? organizationId
            : null;
    }

    private string? GetRoleOrNull()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
    }

    private string GetRequiredRole()
    {
        return GetRoleOrNull()
            ?? throw new UnauthorizedAccessException("Role tidak ditemukan pada token.");
    }
}
