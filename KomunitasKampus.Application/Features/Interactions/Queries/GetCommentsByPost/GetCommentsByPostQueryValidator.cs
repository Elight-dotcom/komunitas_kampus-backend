using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryValidator : AbstractValidator<GetCommentsByPostQuery>
{
    public GetCommentsByPostQueryValidator()
    {
        RuleFor(query => query.PostId)
            .NotEmpty()
            .WithMessage("Post id wajib diisi.");

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page minimal 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size harus di antara 1 sampai 50.");
    }
}
