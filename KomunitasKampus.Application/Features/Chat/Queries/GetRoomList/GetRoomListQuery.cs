using KomunitasKampus.Application.Features.Chat.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Queries.GetRoomList;

public sealed record GetRoomListQuery(Guid AccountId) : IRequest<List<ChatRoomSummaryDto>>;
