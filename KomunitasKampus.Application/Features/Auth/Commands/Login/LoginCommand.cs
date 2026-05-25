using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Identifier,
    string Password
) : IRequest<AuthCommandResult>;