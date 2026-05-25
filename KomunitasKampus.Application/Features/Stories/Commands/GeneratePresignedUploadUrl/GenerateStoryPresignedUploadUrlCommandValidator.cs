using FluentValidation;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Stories.Commands.GeneratePresignedUploadUrl;

public class GenerateStoryPresignedUploadUrlCommandValidator
    : AbstractValidator<GenerateStoryPresignedUploadUrlCommand>
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;

    public GenerateStoryPresignedUploadUrlCommandValidator()
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

        RuleFor(command => command.FileName)
            .NotEmpty()
            .WithMessage("Nama file wajib diisi.");

        RuleFor(command => command.MediaType)
            .Must(mediaType => mediaType is StoryMediaType.Image or StoryMediaType.Video)
            .WithMessage("Presigned URL story hanya mendukung media image atau video.");

        RuleFor(command => command.FileSize)
            .GreaterThan(0)
            .WithMessage("Ukuran file wajib lebih dari 0 byte.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Ukuran file story maksimal 20MB.");

        RuleFor(command => command)
            .Must(HaveAllowedExtensionForMediaType)
            .WithMessage("Ekstensi file story hanya boleh .jpg, .jpeg, .png, atau .mp4 sesuai media type.");
    }

    private static bool HaveAllowedExtensionForMediaType(
        GenerateStoryPresignedUploadUrlCommand command
    )
    {
        var extension = Path
            .GetExtension(command.FileName)
            .ToLowerInvariant();

        return command.MediaType switch
        {
            StoryMediaType.Image => extension is ".jpg" or ".jpeg" or ".png",
            StoryMediaType.Video => extension is ".mp4",
            _ => false
        };
    }
}
