using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetMembershipStatus;

public class GetMembershipStatusQueryValidator : AbstractValidator<GetMembershipStatusQuery>
{
    public GetMembershipStatusQueryValidator()
    {
        RuleFor(query => query.AccountId)
            .NotEmpty()
            .WithMessage("Account id wajib diisi.");

        RuleFor(query => query.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib diisi.");
    }
}
