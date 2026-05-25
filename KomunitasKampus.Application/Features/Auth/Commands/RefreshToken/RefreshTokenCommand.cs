using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthCommandResult>;