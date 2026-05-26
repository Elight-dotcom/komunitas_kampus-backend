using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Commands.SendJoinRequest;

public class SendJoinRequestCommandValidator : AbstractValidator<SendJoinRequestCommand>
{
    public SendJoinRequestCommandValidator()
    {
        RuleFor(command => command.AccountId)
            .NotEmpty()
            .WithMessage("Account id wajib diisi.");

        RuleFor(command => command.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib diisi.");
    }
}
