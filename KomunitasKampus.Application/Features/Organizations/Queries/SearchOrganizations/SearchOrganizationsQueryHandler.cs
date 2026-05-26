using KomunitasKampus.Application.Features.Organizations.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Organizations.Queries.SearchOrganizations;

public class SearchOrganizationsQueryHandler
    : IRequestHandler<SearchOrganizationsQuery, IReadOnlyList<OrganizationCardDto>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public SearchOrganizationsQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<IReadOnlyList<OrganizationCardDto>> Handle(
        SearchOrganizationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var orgs = await _organizationRepository.SearchAsync(
            request.Query,
            request.Page,
            request.PageSize,
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
