using System.Security.Claims;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Features.Notifications.DTOs;
using KomunitasKampus.Application.Features.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetAccountId()
    {
        var accountIdClaim = User.FindFirst("account_id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.Parse(accountIdClaim!);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();

        var results = await _mediator.Send(
            new GetNotificationsQuery(accountId),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(
            results,
            "Notifikasi berhasil diambil."
        ));
    }
}