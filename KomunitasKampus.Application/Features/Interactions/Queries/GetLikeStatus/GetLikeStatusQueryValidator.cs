using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetLikeStatus;

public class GetLikeStatusQueryValidator : AbstractValidator<GetLikeStatusQuery>
{
    public GetLikeStatusQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("User id wajib diisi.");

        RuleFor(query => query.PostId)
            .NotEmpty()
            .WithMessage("Post id wajib diisi.");
    }
}
