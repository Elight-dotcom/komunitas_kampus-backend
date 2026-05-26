using KomunitasKampus.Application.Features.Organizations.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Organizations.Queries.SearchOrganizations;

public record SearchOrganizationsQuery(
    string? Query,
    int Page,
    int PageSize
) : IRequest<IReadOnlyList<OrganizationCardDto>>;
