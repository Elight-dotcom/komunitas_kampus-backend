using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Commands.DeleteMessage;

public class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageCommandValidator()
    {
        RuleFor(command => command.MessageId).NotEmpty().WithMessage("MessageId tidak boleh kosong.");
        RuleFor(command => command.RequestingAccountId).NotEmpty().WithMessage("RequestingAccountId tidak boleh kosong.");
    }
}
