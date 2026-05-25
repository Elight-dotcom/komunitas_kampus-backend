using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetPendingRequests;

public class GetPendingRequestsQueryValidator : AbstractValidator<GetPendingRequestsQuery>
{
    public GetPendingRequestsQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib diisi.");
    }
}
