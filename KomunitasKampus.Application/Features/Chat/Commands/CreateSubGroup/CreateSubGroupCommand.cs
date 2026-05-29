using KomunitasKampus.Application.Features.Chat.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.CreateSubGroup;

public sealed record CreateSubGroupCommand(Guid OrganizationAccountId, string Name, bool IsInviteOnly, List<Guid> MemberAccountIds) : IRequest<ChatRoomDto>;
