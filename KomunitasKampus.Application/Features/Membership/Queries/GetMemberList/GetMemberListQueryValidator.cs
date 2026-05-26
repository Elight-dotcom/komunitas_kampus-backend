using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetMemberList;

public class GetMemberListQueryValidator : AbstractValidator<GetMemberListQuery>
{
    public GetMemberListQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib diisi.");
    }
}
