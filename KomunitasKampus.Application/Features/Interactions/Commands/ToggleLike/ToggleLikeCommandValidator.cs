using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Commands.ToggleLike;

public class ToggleLikeCommandValidator : AbstractValidator<ToggleLikeCommand>
{
    public ToggleLikeCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("User id wajib diisi.");

        RuleFor(command => command.PostId)
            .NotEmpty()
            .WithMessage("Post id wajib diisi.");
    }
}
