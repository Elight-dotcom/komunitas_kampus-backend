using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Commands.DeleteOwnComment;

public class DeleteOwnCommentCommandValidator : AbstractValidator<DeleteOwnCommentCommand>
{
    public DeleteOwnCommentCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("User id wajib diisi.");

        RuleFor(command => command.CommentId)
            .NotEmpty()
            .WithMessage("Comment id wajib diisi.");
    }
}
