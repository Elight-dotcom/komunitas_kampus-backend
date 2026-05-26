using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Features.Organizations.DTOs;
using KomunitasKampus.Application.Features.Organizations.Queries.GetOrganizationById;
using KomunitasKampus.Application.Features.Organizations.Queries.GetRecommendedOrganizations;
using KomunitasKampus.Application.Features.Organizations.Queries.SearchOrganizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[Route("api/explore")]
[AllowAnonymous]
public class ExploreController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExploreController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cari organisasi berdasarkan nama, slug, universitas, atau deskripsi.
    /// Jika query kosong, kembalikan semua (paginasi).
    /// </summary>
    [HttpGet("organizations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrganizationCardDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOrganizations(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        var results = await _mediator.Send(
            new SearchOrganizationsQuery(q, page, pageSize),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<OrganizationCardDto>>.Ok(
            results,
            "Pencarian organisasi berhasil."
        ));
    }

    /// <summary>
    /// Rekomendasi organisasi secara acak (untuk tampil saat input masih kosong).
    /// </summary>
    [HttpGet("organizations/recommended")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrganizationCardDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecommended(
        [FromQuery] int limit = 12,
        CancellationToken cancellationToken = default
    )
    {
        var results = await _mediator.Send(
            new GetRecommendedOrganizationsQuery(limit),
            cancellationToken
        );

        return Ok(ApiResponse<IReadOnlyList<OrganizationCardDto>>.Ok(
            results,
            "Rekomendasi organisasi berhasil diambil."
        ));
    }

    /// <summary>
    /// Ambil detail organisasi berdasarkan ID.
    /// </summary>
    [HttpGet("organizations/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mediator.Send(
            new GetOrganizationByIdQuery(id),
            cancellationToken
        );

        if (result == null)
        {
            return NotFound(ApiResponse<OrganizationDetailDto>.Fail(
                "Organisasi tidak ditemukan."
            ));
        }

        return Ok(ApiResponse<OrganizationDetailDto>.Ok(
            result,
            "Detail organisasi berhasil diambil."
        ));
    }
}
