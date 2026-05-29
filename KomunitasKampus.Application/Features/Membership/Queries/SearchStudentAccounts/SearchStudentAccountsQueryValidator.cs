using FluentValidation;

namespace KomunitasKampus.Application.Features.Membership.Queries.SearchStudentAccounts;

public class SearchStudentAccountsQueryValidator : AbstractValidator<SearchStudentAccountsQuery>
{
    public SearchStudentAccountsQueryValidator()
    {
        RuleFor(query => query.Username)
            .NotEmpty().WithMessage("Username pencarian wajib diisi.")
            .MinimumLength(2).WithMessage("Username pencarian minimal 2 karakter.")
            .MaximumLength(50).WithMessage("Username pencarian maksimal 50 karakter.");

        RuleFor(query => query.Limit)
            .InclusiveBetween(1, 20).WithMessage("Limit pencarian harus antara 1 dan 20.");
    }
}