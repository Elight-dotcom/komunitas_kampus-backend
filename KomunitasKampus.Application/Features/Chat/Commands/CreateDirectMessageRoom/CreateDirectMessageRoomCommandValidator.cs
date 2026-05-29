using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Commands.CreateDirectMessageRoom;

public class CreateDirectMessageRoomCommandValidator : AbstractValidator<CreateDirectMessageRoomCommand>
{
    public CreateDirectMessageRoomCommandValidator()
    {
        RuleFor(command => command.InitiatorAccountId).NotEmpty().WithMessage("InitiatorAccountId tidak boleh kosong.");
        RuleFor(command => command.TargetAccountId).NotEmpty().WithMessage("TargetAccountId tidak boleh kosong.");
        RuleFor(command => command).Must(command => command.InitiatorAccountId != command.TargetAccountId)
            .WithMessage("InitiatorAccountId dan TargetAccountId tidak boleh sama.");
    }
}
