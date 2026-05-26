using KomunitasKampus.Application.Features.Organizations.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Organizations.Queries.GetOrganizationById;

public record GetOrganizationByIdQuery(Guid Id)
    : IRequest<OrganizationDetailDto?>;