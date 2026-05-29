using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.RoomId).NotEmpty().WithMessage("RoomId tidak boleh kosong.");
        RuleFor(command => command.SenderId).NotEmpty().WithMessage("SenderId tidak boleh kosong.");
        RuleFor(command => command.Content)
            .Must(content => !string.IsNullOrWhiteSpace(content)).WithMessage("Content tidak boleh kosong.")
            .MaximumLength(2000).WithMessage("Content maksimal 2000 karakter.");
    }
}
