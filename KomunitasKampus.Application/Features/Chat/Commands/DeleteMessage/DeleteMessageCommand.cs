using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.DeleteMessage;

public sealed record DeleteMessageCommand(Guid MessageId, Guid RequestingAccountId) : IRequest<Unit>;
