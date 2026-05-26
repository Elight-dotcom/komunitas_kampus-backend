using FluentValidation;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetFeed;

public class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .NotEmpty().WithMessage("OrganizationId wajib diisi.");

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page minimal 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize harus di antara 1 sampai 50.");
    }
}
