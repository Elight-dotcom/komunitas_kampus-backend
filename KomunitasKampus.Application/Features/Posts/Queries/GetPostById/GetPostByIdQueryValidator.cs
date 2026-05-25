using FluentValidation;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryValidator : AbstractValidator<GetPostByIdQuery>
{
    public GetPostByIdQueryValidator()
    {
        RuleFor(query => query.PostId)
            .NotEmpty().WithMessage("PostId wajib diisi.");
    }
}
