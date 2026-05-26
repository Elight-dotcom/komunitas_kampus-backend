using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Commands.SharePost;

public class SharePostCommandValidator : AbstractValidator<SharePostCommand>
{
    public SharePostCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("User id wajib diisi.");

        RuleFor(command => command.PostId)
            .NotEmpty()
            .WithMessage("Post id wajib diisi.");

        RuleFor(command => command.Platform)
            .IsInEnum()
            .WithMessage("Platform share tidak valid.");
    }
}
