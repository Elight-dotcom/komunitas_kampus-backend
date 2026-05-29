using KomunitasKampus.Application.Features.Chat.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Queries.GetMessages;

public sealed record GetMessagesQuery(Guid RoomId, Guid RequestingAccountId, int Page = 1, int PageSize = 30) : IRequest<PagedResult<MessageDto>>;
