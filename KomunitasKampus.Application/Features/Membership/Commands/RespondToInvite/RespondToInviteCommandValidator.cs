using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Commands.RespondToInvite;

public class RespondToInviteCommandValidator : AbstractValidator<RespondToInviteCommand>
{
    public RespondToInviteCommandValidator()
    {
        RuleFor(command => command.MembershipId)
            .NotEmpty()
            .WithMessage("Membership id wajib diisi.");

        RuleFor(command => command.AccountId)
            .NotEmpty()
            .WithMessage("Account id wajib diisi.");

        RuleFor(command => command.Action)
            .NotEmpty()
            .WithMessage("Action wajib diisi.")
            .Must(action =>
            {
                var normalized = action.Trim().ToLowerInvariant();

                return normalized is "accept" or "accepted" or "approve" or "approved"
                    or "reject" or "rejected";
            })
            .WithMessage("Action hanya boleh accept atau reject.");
    }
}
