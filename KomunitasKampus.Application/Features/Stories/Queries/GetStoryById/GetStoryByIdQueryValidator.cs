using FluentValidation;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetStoryById;

public class GetStoryByIdQueryValidator
    : AbstractValidator<GetStoryByIdQuery>
{
    public GetStoryByIdQueryValidator()
    {
        RuleFor(query => query.StoryId)
            .NotEmpty()
            .WithMessage("Story id wajib diisi.");
    }
}
