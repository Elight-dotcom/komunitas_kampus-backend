using FluentValidation;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Stories.Commands.CreateStory;

public class CreateStoryCommandValidator : AbstractValidator<CreateStoryCommand>
{
    public CreateStoryCommandValidator()
    {
        RuleFor(command => command.RequesterAccountId)
            .NotEmpty()
            .WithMessage("Requester account id wajib diisi.");

        RuleFor(command => command.RequesterRole)
            .NotEmpty()
            .WithMessage("Role requester wajib diisi.");

        RuleFor(command => command.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id wajib diisi.");

        RuleFor(command => command.MediaType)
            .IsInEnum()
            .WithMessage("Media type tidak valid.");

        When(command => command.MediaType == StoryMediaType.Text, () =>
        {
            RuleFor(command => command.TextContent)
                .NotEmpty()
                .WithMessage("Text content wajib diisi untuk story tipe text.")
                .MaximumLength(280)
                .WithMessage("Text content maksimal 280 karakter.");

            RuleFor(command => command.FileKey)
                .Must(string.IsNullOrWhiteSpace)
                .WithMessage("File key harus kosong untuk story tipe text.");
        });

        When(command => command.MediaType is StoryMediaType.Image or StoryMediaType.Video, () =>
        {
            RuleFor(command => command.FileKey)
                .NotEmpty()
                .WithMessage("File key wajib diisi untuk story tipe image/video.");

            RuleFor(command => command.TextContent)
                .MaximumLength(280)
                .WithMessage("Text content maksimal 280 karakter.");
        });
    }
}
