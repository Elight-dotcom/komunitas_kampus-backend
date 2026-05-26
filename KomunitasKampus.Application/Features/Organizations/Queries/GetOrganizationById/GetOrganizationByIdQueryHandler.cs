using KomunitasKampus.Application.Features.Organizations.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Organizations.Queries.GetOrganizationById;

public class GetOrganizationByIdQueryHandler
    : IRequestHandler<GetOrganizationByIdQuery, OrganizationDetailDto?>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetOrganizationByIdQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<OrganizationDetailDto?> Handle(
        GetOrganizationByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var org = await _organizationRepository.GetByIdAsync(
            request.Id,
            cancellationToken
        );

        if (org == null) return null;

        return new OrganizationDetailDto(
            org.Id,
            org.OrganizationName,
            org.Slug,
            org.University,
            org.Description,
            org.Account?.AvatarUrl,
            null, // BannerUrl - not yet in entity
            org.Memberships.Count(m => m.DeletedAt == null),
            org.Posts.Count(p => p.DeletedAt == null),
            org.CreatedAt
        );
    }
}