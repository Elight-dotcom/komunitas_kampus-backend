using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Queries.CheckUsernameAvailability;

public sealed record CheckUsernameAvailabilityQuery(
    string Username
) : IRequest<UsernameAvailabilityDto>;