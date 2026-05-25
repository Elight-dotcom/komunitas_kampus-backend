using FluentValidation;

namespace KomunitasKampus.Application.Features.Stories.Queries.GetActiveStories;

public class GetActiveStoriesQueryValidator
    : AbstractValidator<GetActiveStoriesQuery>
{
    public GetActiveStoriesQueryValidator()
    {
        // ViewerAccountId boleh null untuk public viewer.
    }
}
