using FluentValidation;

namespace KomunitasKampus.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(command => command.PostId)
            .NotEmpty().WithMessage("PostId wajib diisi.");

        RuleFor(command => command.RequesterAccountId)
            .NotEmpty().WithMessage("RequesterAccountId wajib diisi.");

        RuleFor(command => command.RequesterRole)
            .NotEmpty().WithMessage("RequesterRole wajib diisi.");
    }
}
