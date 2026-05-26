using FluentValidation;

namespace KomunitasKampus.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(command => command.PostId)
            .NotEmpty().WithMessage("PostId wajib diisi.");

        RuleFor(command => command.RequesterAccountId)
            .NotEmpty().WithMessage("RequesterAccountId wajib diisi.");

        RuleFor(command => command.RequesterRole)
            .NotEmpty().WithMessage("RequesterRole wajib diisi.");

        RuleFor(command => command.Payload.Title)
            .NotEmpty().WithMessage("Title wajib diisi.")
            .MaximumLength(200).WithMessage("Title maksimal 200 karakter.");

        RuleFor(command => command.Payload.Caption)
            .MaximumLength(5000).WithMessage("Caption maksimal 5000 karakter.");
    }
}
