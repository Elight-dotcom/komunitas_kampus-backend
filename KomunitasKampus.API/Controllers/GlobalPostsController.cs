using System.Security.Claims;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Application.Features.Posts.Queries.GetGlobalFeed;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[Route("api/posts")]
public class GlobalPostsController : ControllerBase
{
    private readonly ISender _sender;

    public GlobalPostsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("global")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PostDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobalFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var query = new GetGlobalFeedQuery(
            ViewerAccountId: GetAccountIdOrNull(),
            ViewerRole: GetRoleOrNull(),
            ViewerOrganizationId: GetOrganizationIdOrNull(),
            Page: page,
            PageSize: pageSize
        );

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<PostDto>>.Ok(
            result,
            "Global feed berhasil diambil."
        ));
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
}