using FluentValidation;

namespace KomunitasKampus.Application.Features.Posts.Commands.GeneratePresignedUploadUrl;

public class GeneratePresignedUploadUrlCommandValidator
    : AbstractValidator<GeneratePresignedUploadUrlCommand>
{
    private const int MaxFileSize = 20 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".mp4",
        ".pdf"
    ];

    public GeneratePresignedUploadUrlCommandValidator()
    {
        RuleFor(command => command.FileName)
            .NotEmpty().WithMessage("Nama file wajib diisi.")
            .Must(HaveAllowedExtension)
            .WithMessage("Ekstensi file hanya boleh .jpg, .jpeg, .png, .mp4, atau .pdf.");

        RuleFor(command => command.FileSize)
            .GreaterThan(0).WithMessage("Ukuran file tidak valid.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Ukuran file maksimal 20MB.");

        RuleFor(command => command.RequesterRole)
            .NotEmpty().WithMessage("Role pengguna tidak ditemukan.");

        RuleFor(command => command.RequesterOrganizationId)
            .NotNull().WithMessage("Organization id pada token tidak ditemukan.");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return AllowedExtensions.Contains(extension);
    }
}
