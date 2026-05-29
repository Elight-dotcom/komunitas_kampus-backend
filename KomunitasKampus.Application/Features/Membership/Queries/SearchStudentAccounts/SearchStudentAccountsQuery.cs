using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.SearchStudentAccounts;

public sealed record SearchStudentAccountsQuery(
    string Username,
    int Limit = 10
) : IRequest<IReadOnlyList<StudentSearchResultDto>>;