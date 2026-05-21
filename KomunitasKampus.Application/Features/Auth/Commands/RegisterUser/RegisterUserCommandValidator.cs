using FluentValidation;
using KomunitasKampus.Domain.Repositories;

namespace KomunitasKampus.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IAccountRepository _accountRepository;

    public RegisterUserCommandValidator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;

        RuleFor(command => command.FullName)
            .NotEmpty().WithMessage("Nama lengkap wajib diisi.")
            .MaximumLength(150).WithMessage("Nama lengkap maksimal 150 karakter.");

        RuleFor(command => command.University)
            .NotEmpty().WithMessage("Universitas wajib diisi.")
            .MaximumLength(100).WithMessage("Universitas maksimal 100 karakter.");

        RuleFor(command => command.Username)
            .NotEmpty().WithMessage("Username wajib diisi.")
            .MinimumLength(3).WithMessage("Username minimal 3 karakter.")
            .MaximumLength(50).WithMessage("Username maksimal 50 karakter.")
            .Matches("^[a-zA-Z0-9._]+$").WithMessage("Username hanya boleh berisi huruf, angka, titik, dan underscore.")
            .MustAsync(BeUniqueUsername).WithMessage("Username sudah digunakan.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email wajib diisi.")
            .EmailAddress().WithMessage("Format email tidak valid.")
            .Must(UseAcademicDomain).WithMessage("Email mahasiswa harus menggunakan domain kampus .ac.id.")
            .MustAsync(BeUniqueEmail).WithMessage("Email sudah terdaftar.");

        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("Password wajib diisi.")
            .MinimumLength(8).WithMessage("Password minimal 8 karakter.")
            .Matches("[A-Z]").WithMessage("Password harus memiliki minimal 1 huruf besar.")
            .Matches("[a-z]").WithMessage("Password harus memiliki minimal 1 huruf kecil.")
            .Matches("[0-9]").WithMessage("Password harus memiliki minimal 1 angka.");

        RuleFor(command => command.ConfirmPassword)
            .Equal(command => command.Password).WithMessage("Confirm password tidak sama dengan password.");
    }

    private async Task<bool> BeUniqueUsername(
        string username,
        CancellationToken cancellationToken
    )
    {
        return !await _accountRepository.IsUsernameTakenAsync(
            username.Trim(),
            cancellationToken
        );
    }

    private async Task<bool> BeUniqueEmail(
        string email,
        CancellationToken cancellationToken
    )
    {
        return !await _accountRepository.IsEmailTakenAsync(
            email.Trim().ToLowerInvariant(),
            cancellationToken
        );
    }

    private static bool UseAcademicDomain(string email)
    {
        return email.Trim().ToLowerInvariant().Contains(".ac.id");
    }
}