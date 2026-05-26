using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Commands.SendInvite;

public class SendInviteCommandValidator : AbstractValidator<SendInviteCommand>
{
    public SendInviteCommandValidator()
    {
        RuleFor(command => command.RequesterAccountId)
            .NotEmpty()
            .WithMessage("Requester account id wajib diisi.");

        RuleFor(command => command.RequesterRole)
            .NotEmpty()
            .WithMessage("Requester role wajib diisi.");

        RuleFor(command => command.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib diisi.");

        RuleFor(command => command.TargetUsername)
            .NotEmpty()
            .WithMessage("Username target wajib diisi.")
            .MaximumLength(100)
            .WithMessage("Username target maksimal 100 karakter.");
    }
}
