using FluentValidation;

namespace KomunitasKampus.Application.Features.Stories.Commands.MarkStoryViewed;

public class MarkStoryViewedCommandValidator
    : AbstractValidator<MarkStoryViewedCommand>
{
    public MarkStoryViewedCommandValidator()
    {
        RuleFor(command => command.StoryId)
            .NotEmpty()
            .WithMessage("Story id wajib diisi.");

        RuleFor(command => command.AccountId)
            .NotEmpty()
            .WithMessage("Account id wajib diisi.");
    }
}
