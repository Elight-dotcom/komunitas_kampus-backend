using FluentValidation;

namespace KomunitasKampus.Application.Features.Posts.Commands.TogglePostPin;

public class TogglePostPinCommandValidator : AbstractValidator<TogglePostPinCommand>
{
    public TogglePostPinCommandValidator()
    {
        RuleFor(command => command.PinOrder)
            .InclusiveBetween(1, 3)
            .When(command => command.IsPinned && command.PinOrder.HasValue)
            .WithMessage("Pin order hanya boleh 1, 2, atau 3.");

        RuleFor(command => command.PinOrder)
            .NotNull()
            .When(command => command.IsPinned)
            .WithMessage("Pin order wajib diisi saat post di-pin.");
    }
}
