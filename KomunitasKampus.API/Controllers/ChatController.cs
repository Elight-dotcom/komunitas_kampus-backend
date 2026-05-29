using System.Security.Claims;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Chat.Commands.CreateDirectMessageRoom;
using KomunitasKampus.Application.Features.Chat.Commands.CreateSubGroup;
using KomunitasKampus.Application.Features.Chat.Commands.DeleteMessage;
using KomunitasKampus.Application.Features.Chat.Commands.MarkRoomAsRead;
using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Application.Features.Chat.Queries.GetMessages;
using KomunitasKampus.Application.Features.Chat.Queries.GetRoomList;
using KomunitasKampus.Application.Features.Chat.Queries.SearchChatUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ISender _sender;

    public ChatController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Ambil daftar semua room yang diikuti user yang sedang login
    /// </summary>
    [HttpGet("rooms")]
    [ProducesResponseType(typeof(ApiResponse<List<ChatRoomSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomList(CancellationToken cancellationToken)
    {
        var accountId = GetRequiredAccountId();

        var result = await _sender.Send(
            new GetRoomListQuery(accountId),
            cancellationToken
        );

        return Ok(ApiResponse<List<ChatRoomSummaryDto>>.Ok(
            result,
            "Daftar room berhasil diambil."
        ));
    }

    /// <summary>
    /// Ambil pesan dalam sebuah room (paginated, terbaru di halaman pertama)
    /// </summary>
    [HttpGet("rooms/{roomId:guid}/messages")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMessages(
        Guid roomId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default
    )
    {
        var accountId = GetRequiredAccountId();

        try
        {
            var result = await _sender.Send(
                new GetMessagesQuery(
                    RoomId: roomId,
                    RequestingAccountId: accountId,
                    Page: page,
                    PageSize: pageSize
                ),
                cancellationToken
            );

            return Ok(ApiResponse<PagedResult<MessageDto>>.Ok(
                result,
                "Pesan berhasil diambil."
            ));
        }
        catch (UnauthorizedAppException)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail("Anda bukan participant di room ini.")
            );
        }
    }

    /// <summary>
    /// Inisiasi atau buka kembali DM dengan user lain
    /// </summary>
    [HttpPost("rooms/direct")]
    [ProducesResponseType(typeof(ApiResponse<ChatRoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDirectMessageRoom(
        [FromBody] CreateDirectMessageRoomRequest request,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        var result = await _sender.Send(
            new CreateDirectMessageRoomCommand(
                InitiatorAccountId: accountId,
                TargetAccountId: request.TargetAccountId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<ChatRoomDto>.Ok(
            result,
            "Room DM berhasil dibuat atau ditemukan."
        ));
    }

    /// <summary>
    /// Cari user mahasiswa berdasarkan username untuk memulai DM
    /// </summary>
    [HttpGet("users/search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatUserSearchResultDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsersByUsername(
        [FromQuery] string username,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default
    )
    {
        var accountId = GetRequiredAccountId();

        var result = await _sender.Send(
            new SearchChatUsersQuery(
                RequesterAccountId: accountId,
                Username: username,
                Limit: limit
            ),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<ChatUserSearchResultDto>>.Ok(
            result,
            "Pencarian user berhasil diambil."
        ));
    }

    /// <summary>
    /// Buat sub-grup baru (hanya untuk role organization)
    /// </summary>
    [HttpPost("rooms/sub-group")]
    [Authorize(Roles = "organization")]
    [ProducesResponseType(typeof(ApiResponse<ChatRoomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSubGroup(
        [FromBody] CreateSubGroupRequest request,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        var result = await _sender.Send(
            new CreateSubGroupCommand(
                OrganizationAccountId: accountId,
                Name: request.Name,
                IsInviteOnly: request.IsInviteOnly,
                MemberAccountIds: request.MemberAccountIds.ToList()
            ),
            cancellationToken
        );

        return Ok(ApiResponse<ChatRoomDto>.Ok(
            result,
            "Sub-grup berhasil dibuat."
        ));
    }

    /// <summary>
    /// Hapus pesan (soft delete)
    /// </summary>
    [HttpDelete("messages/{messageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMessage(
        Guid messageId,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        await _sender.Send(
            new DeleteMessageCommand(
                MessageId: messageId,
                RequestingAccountId: accountId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Message deleted successfully"
        ));
    }

    /// <summary>
    /// Tandai semua pesan di room sebagai sudah dibaca
    /// </summary>
    [HttpPost("rooms/{roomId:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRoomAsRead(
        Guid roomId,
        CancellationToken cancellationToken
    )
    {
        var accountId = GetRequiredAccountId();

        await _sender.Send(
            new MarkRoomAsReadCommand(
                RoomId: roomId,
                AccountId: accountId
            ),
            cancellationToken
        );

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Marked as read"
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
}

public sealed record CreateDirectMessageRoomRequest(Guid TargetAccountId);

public sealed record CreateSubGroupRequest(
    string Name,
    bool IsInviteOnly,
    Guid[] MemberAccountIds
);