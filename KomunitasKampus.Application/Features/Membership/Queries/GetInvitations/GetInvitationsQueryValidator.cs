using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetInvitations;

public class GetInvitationsQueryValidator : AbstractValidator<GetInvitationsQuery>
{
    public GetInvitationsQueryValidator()
    {
        RuleFor(query => query.AccountId)
            .NotEmpty()
            .WithMessage("Account id wajib diisi.");
    }
}
