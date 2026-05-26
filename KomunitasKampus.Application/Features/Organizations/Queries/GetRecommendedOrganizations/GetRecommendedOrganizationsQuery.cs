using KomunitasKampus.Application.Features.Organizations.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Organizations.Queries.GetRecommendedOrganizations;

public record GetRecommendedOrganizationsQuery(
    int Limit
) : IRequest<IReadOnlyList<OrganizationCardDto>>;
