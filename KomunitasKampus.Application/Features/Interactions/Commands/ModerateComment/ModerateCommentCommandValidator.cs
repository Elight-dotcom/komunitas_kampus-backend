using FluentValidation;

namespace KomunitasKampus.Application.Features.Interactions.Commands.ModerateComment;

public class ModerateCommentCommandValidator : AbstractValidator<ModerateCommentCommand>
{
    public ModerateCommentCommandValidator()
    {
        RuleFor(command => command.RequesterAccountId)
            .NotEmpty()
            .WithMessage("Requester account id wajib diisi.");

        RuleFor(command => command.RequesterOrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib ada di token.");

        RuleFor(command => command.PostId)
            .NotEmpty()
            .WithMessage("Post id wajib diisi.");

        RuleFor(command => command.CommentId)
            .NotEmpty()
            .WithMessage("Comment id wajib diisi.");

        RuleFor(command => command.DeletedReason)
            .NotEmpty()
            .WithMessage("Alasan moderasi wajib diisi.")
            .MaximumLength(150)
            .WithMessage("Alasan moderasi maksimal 150 karakter.");
    }
}
