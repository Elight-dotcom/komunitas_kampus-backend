using FluentValidation;

namespace KomunitasKampus.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Identifier)
            .NotEmpty().WithMessage("Email atau username wajib diisi.");

        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("Password wajib diisi.");
    }
}