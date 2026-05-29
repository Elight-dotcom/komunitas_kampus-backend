using FluentValidation;

namespace KomunitasKampus.Application.Features.Chat.Commands.CreateSubGroup;

public class CreateSubGroupCommandValidator : AbstractValidator<CreateSubGroupCommand>
{
    public CreateSubGroupCommandValidator()
    {
        RuleFor(command => command.OrganizationAccountId).NotEmpty().WithMessage("OrganizationAccountId tidak boleh kosong.");
        RuleFor(command => command.Name).NotEmpty().WithMessage("Name wajib diisi.").MaximumLength(100).WithMessage("Name maksimal 100 karakter.");
        RuleFor(command => command.MemberAccountIds).NotNull().WithMessage("MemberAccountIds tidak boleh null.");
        RuleForEach(command => command.MemberAccountIds).NotEmpty().WithMessage("MemberAccountIds tidak boleh berisi Guid.Empty.");
    }
}
