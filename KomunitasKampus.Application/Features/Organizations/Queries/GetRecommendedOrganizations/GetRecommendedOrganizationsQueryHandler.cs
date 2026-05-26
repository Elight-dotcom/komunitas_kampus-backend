using KomunitasKampus.Application.Features.Organizations.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Organizations.Queries.GetRecommendedOrganizations;

public class GetRecommendedOrganizationsQueryHandler
    : IRequestHandler<GetRecommendedOrganizationsQuery, IReadOnlyList<OrganizationCardDto>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetRecommendedOrganizationsQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<IReadOnlyList<OrganizationCardDto>> Handle(
        GetRecommendedOrganizationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var orgs = await _organizationRepository.GetRecommendedAsync(
            request.Limit,
            cancellationToken
        );

        return orgs.Select(o => new OrganizationCardDto(
            o.Id,
            o.OrganizationName,
            o.Slug,
            o.University,
            o.Description,
            o.Account?.AvatarUrl,
            o.Memberships.Count(m => m.DeletedAt == null),
            o.Posts.Count(p => p.DeletedAt == null)
        )).ToList();
    }
}
