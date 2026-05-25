using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("User id wajib diisi.");

        RuleFor(command => command.PostId)
            .NotEmpty()
            .WithMessage("Post id wajib diisi.");

        RuleFor(command => command.Content)
            .NotEmpty()
            .WithMessage("Komentar tidak boleh kosong.")
            .MaximumLength(500)
            .WithMessage("Komentar maksimal 500 karakter.");
    }
}
