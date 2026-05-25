using System.Security.Claims;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Membership.Commands.ResolveMembership;
using KomunitasKampus.Application.Features.Membership.Commands.RespondToInvite;
using KomunitasKampus.Application.Features.Membership.Commands.SendInvite;
using KomunitasKampus.Application.Features.Membership.Commands.SendJoinRequest;
using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Application.Features.Membership.Queries.GetInvitations;
using KomunitasKampus.Application.Features.Membership.Queries.GetMemberList;
using KomunitasKampus.Application.Features.Membership.Queries.GetMembershipStatus;
using KomunitasKampus.Application.Features.Membership.Queries.GetPendingRequests;
using KomunitasKampus.API.Contracts.Membership;
using KomunitasKampus.API.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
public class MembershipController : ControllerBase
{
    private const string StudentRoles = "user,mahasiswa,student";
    private const string OrganizationRoles = "organization,organisasi";

    private readonly IMediator _mediator;

    public MembershipController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // =========================
    // Sisi Mahasiswa
    // =========================

    [Authorize(Roles = StudentRoles)]
    [HttpPost("api/organizations/{orgId:guid}/join")]
    [ProducesResponseType(typeof(ApiResponse<MembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendJoinRequest(
        Guid orgId,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        try
        {
            var result = await _mediator.Send(
                new SendJoinRequestCommand(
                    AccountId: accountId,
                    OrganizationId: orgId
                ),
                cancellationToken
            );

            return Ok(ApiResponse<MembershipDto>.Ok(
                result,
                "Permintaan bergabung berhasil dikirim."
            ));
        }
        catch (ValidationAppException exception) when (IsDuplicateMembershipError(exception))
        {
            return Conflict(ApiResponse<object>.Fail(
                "Kamu sudah memiliki membership pending atau sudah menjadi anggota organisasi ini.",
                exception.Errors
            ));
        }
        catch (ValidationAppException exception) when (IsCooldownError(exception))
        {
            var status = await _mediator.Send(
                new GetMembershipStatusQuery(accountId, orgId),
                cancellationToken
            );

            var remainingDays = CalculateRemainingCooldownDays(status.CanRequestAgainAt);

            Response.Headers.RetryAfter = TimeSpan.FromDays(Math.Max(1, remainingDays))
                .TotalSeconds
                .ToString("0");

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                ApiResponse<object>.Fail(
                    $"Kamu masih dalam masa cooldown. Coba lagi sekitar {remainingDays} hari lagi.",
                    new Dictionary<string, string[]>
                    {
                        ["cooldown"] = new[]
                        {
                            $"Sisa cooldown sekitar {remainingDays} hari.",
                        }
                    }
                )
            );
        }
    }

    [Authorize]
    [HttpGet("api/organizations/{orgId:guid}/membership-status")]
    [ProducesResponseType(typeof(ApiResponse<MembershipStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembershipStatus(
        Guid orgId,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        var result = await _mediator.Send(
            new GetMembershipStatusQuery(
                AccountId: accountId,
                OrganizationId: orgId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<MembershipStatusDto>.Ok(
            result,
            "Status membership berhasil diambil."
        ));
    }

    [Authorize(Roles = StudentRoles)]
    [HttpGet("api/invitations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InviteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations(CancellationToken cancellationToken)
    {
        var accountId = GetRequiredAccountId();

        var result = await _mediator.Send(
            new GetInvitationsQuery(accountId),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<InviteDto>>.Ok(
            result,
            "Daftar undangan berhasil diambil."
        ));
    }

    [Authorize(Roles = StudentRoles)]
    [HttpPost("api/invitations/{membershipId:guid}/respond")]
    [ProducesResponseType(typeof(ApiResponse<InviteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RespondToInvite(
        Guid membershipId,
        [FromBody] RespondInviteRequest request,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        var result = await _mediator.Send(
            new RespondToInviteCommand(
                MembershipId: membershipId,
                AccountId: accountId,
                Action: request.Action
            ),
            cancellationToken
        );

        return Ok(ApiResponse<InviteDto>.Ok(
            result,
            $"Undangan berhasil di-{request.Action.ToLowerInvariant()}."
        ));
    }

    // =========================
    // Sisi Admin Organisasi
    // =========================

    [Authorize(Roles = OrganizationRoles)]
    [HttpGet("api/organizations/{orgId:guid}/members")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MembershipDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMembers(
        Guid orgId,
        CancellationToken cancellationToken
    )
    {
        EnsureOrganizationAccess(orgId);

        var result = await _mediator.Send(
            new GetMemberListQuery(orgId),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<MembershipDto>>.Ok(
            result,
            "Daftar anggota berhasil diambil."
        ));
    }

    [Authorize(Roles = OrganizationRoles)]
    [HttpGet("api/organizations/{orgId:guid}/requests")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MemberRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingRequests(
        Guid orgId,
        CancellationToken cancellationToken
    )
    {
        EnsureOrganizationAccess(orgId);

        var result = await _mediator.Send(
            new GetPendingRequestsQuery(orgId),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<MemberRequestDto>>.Ok(
            result,
            "Daftar request membership berhasil diambil."
        ));
    }

    [Authorize(Roles = OrganizationRoles)]
    [HttpPost("api/organizations/{orgId:guid}/invite")]
    [ProducesResponseType(typeof(ApiResponse<InviteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendInvite(
        Guid orgId,
        [FromBody] SendInviteRequest request,
        CancellationToken cancellationToken
    )
    {
        EnsureOrganizationAccess(orgId);

        var result = await _mediator.Send(
            new SendInviteCommand(
                RequesterAccountId: GetRequiredAccountId(),
                RequesterRole: GetRequiredRole(),
                RequesterOrganizationId: GetOrganizationIdFromClaims(),
                OrganizationId: orgId,
                TargetUsername: request.Username
            ),
            cancellationToken
        );

        return Ok(ApiResponse<InviteDto>.Ok(
            result,
            "Undangan berhasil dikirim."
        ));
    }

    [Authorize(Roles = OrganizationRoles)]
    [HttpPatch("api/organizations/{orgId:guid}/requests/{membershipId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveMembershipRequest(
        Guid orgId,
        Guid membershipId,
        [FromBody] ResolveMembershipRequest request,
        CancellationToken cancellationToken
    )
    {
        EnsureOrganizationAccess(orgId);

        var result = await _mediator.Send(
            new ResolveMembershipCommand(
                MembershipId: membershipId,
                Action: request.Action,
                ResolvedBy: GetRequiredAccountId()
            ),
            cancellationToken
        );

        return Ok(ApiResponse<MembershipDto>.Ok(
            result,
            $"Request membership berhasil di-{request.Action.ToLowerInvariant()}."
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

    private string GetRequiredRole()
    {
        return User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role")
            ?? string.Empty;
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

    private void EnsureOrganizationAccess(Guid organizationIdFromRoute)
    {
        var organizationIdFromToken = GetOrganizationIdFromClaims();

        if (organizationIdFromToken != organizationIdFromRoute)
        {
            throw new ForbiddenAccessAppException(
                "Akun organisasi tidak boleh mengakses data organisasi lain."
            );
        }
    }

    private static bool IsDuplicateMembershipError(ValidationAppException exception)
    {
        return exception.Errors.Values
            .SelectMany(messages => messages)
            .Any(message =>
                message.Contains("pending", StringComparison.OrdinalIgnoreCase)
                || message.Contains("anggota", StringComparison.OrdinalIgnoreCase)
                || message.Contains("sudah", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static bool IsCooldownError(ValidationAppException exception)
    {
        return exception.Errors.Values
            .SelectMany(messages => messages)
            .Any(message =>
                message.Contains("cooldown", StringComparison.OrdinalIgnoreCase)
                || message.Contains("3 hari", StringComparison.OrdinalIgnoreCase)
                || message.Contains("menunggu", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static int CalculateRemainingCooldownDays(DateTime? canRequestAgainAt)
    {
        if (!canRequestAgainAt.HasValue)
        {
            return 3;
        }

        var remaining = canRequestAgainAt.Value - DateTime.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling(remaining.TotalDays));
    }
}
