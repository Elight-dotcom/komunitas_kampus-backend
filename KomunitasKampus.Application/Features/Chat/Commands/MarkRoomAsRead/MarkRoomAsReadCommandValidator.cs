using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Commands.MarkRoomAsRead;

public class MarkRoomAsReadCommandValidator : AbstractValidator<MarkRoomAsReadCommand>
{
    public MarkRoomAsReadCommandValidator()
    {
        RuleFor(command => command.RoomId).NotEmpty().WithMessage("RoomId tidak boleh kosong.");
        RuleFor(command => command.AccountId).NotEmpty().WithMessage("AccountId tidak boleh kosong.");
    }
}
