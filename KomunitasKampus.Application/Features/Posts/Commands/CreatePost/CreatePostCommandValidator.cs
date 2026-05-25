using FluentValidation;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;

namespace KomunitasKampus.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    private const int MaxFileSizeBytes = 20 * 1024 * 1024;

    private readonly IPostRepository _postRepository;

    public CreatePostCommandValidator(IPostRepository postRepository)
    {
        _postRepository = postRepository;

        RuleFor(command => command.RequesterAccountId)
            .NotEmpty().WithMessage("RequesterAccountId wajib diisi.");

        RuleFor(command => command.RequesterRole)
            .NotEmpty().WithMessage("RequesterRole wajib diisi.");

        RuleFor(command => command.RequesterOrganizationId)
            .NotNull().WithMessage("Akun organisasi wajib memiliki OrganizationId.");

        RuleFor(command => command.Payload.Title)
            .NotEmpty().WithMessage("Title wajib diisi.")
            .MaximumLength(200).WithMessage("Title maksimal 200 karakter.");

        RuleFor(command => command.Payload.Caption)
            .MaximumLength(5000).WithMessage("Caption maksimal 5000 karakter.");

        RuleFor(command => command.Payload.Visibility)
            .Must(BeValidVisibility).WithMessage("Visibility harus salah satu dari: Private, Internal, Public.");

        RuleFor(command => command.Payload.PinOrder)
            .InclusiveBetween(1, 3)
            .When(command => command.Payload.PinOrder.HasValue)
            .WithMessage("Pin order hanya boleh 1, 2, atau 3.");

        RuleFor(command => command)
            .MustAsync(NotExceedPinnedLimit)
            .When(command => command.Payload.IsPinned && command.RequesterOrganizationId.HasValue)
            .WithMessage("Maksimal hanya boleh ada 3 post yang dipin dalam satu organisasi.");

        RuleFor(command => command.Payload.Media)
            .NotEmpty().WithMessage("Minimal harus ada satu media.");

        RuleForEach(command => command.Payload.Media)
            .ChildRules(media =>
            {
                media.RuleFor(item => item.FileName)
                    .NotEmpty().WithMessage("FileName wajib diisi.");

                media.RuleFor(item => item.MediaType)
                    .Must(BeValidMediaType).WithMessage("MediaType harus salah satu dari: Image, Video, Document.");

                media.RuleFor(item => item.FileSizeBytes)
                    .InclusiveBetween(1, MaxFileSizeBytes)
                    .WithMessage("Ukuran file maksimal 20MB.");

                media.RuleFor(item => item.OrderIndex)
                    .GreaterThanOrEqualTo(0).WithMessage("OrderIndex minimal 0.");
            });

        RuleFor(command => command.Payload.Media)
            .Must(HaveSingleMediaType)
            .WithMessage("Satu postingan hanya boleh memakai satu tipe media utama.");
    }

    private static bool BeValidVisibility(string visibility)
    {
        return Enum.TryParse<PostVisibility>(visibility, ignoreCase: true, out _);
    }

    private static bool BeValidMediaType(string mediaType)
    {
        return Enum.TryParse<PostMediaType>(mediaType, ignoreCase: true, out _);
    }

    private static bool HaveSingleMediaType(IReadOnlyList<CreatePostMediaDto> media)
    {
        if (media.Count == 0)
        {
            return true;
        }

        return media
            .Select(item => item.MediaType.Trim().ToLowerInvariant())
            .Distinct()
            .Count() == 1;
    }

    private async Task<bool> NotExceedPinnedLimit(
        CreatePostCommand command,
        CancellationToken cancellationToken
    )
    {
        var pinnedCount = await _postRepository.CountPinnedPostsAsync(
            command.RequesterOrganizationId!.Value,
            excludedPostId: null,
            cancellationToken
        );

        return pinnedCount < 3;
    }
}
