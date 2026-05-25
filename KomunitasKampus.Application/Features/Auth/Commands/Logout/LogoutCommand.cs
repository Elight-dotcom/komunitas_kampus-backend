using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand : IRequest<LogoutResponse>;