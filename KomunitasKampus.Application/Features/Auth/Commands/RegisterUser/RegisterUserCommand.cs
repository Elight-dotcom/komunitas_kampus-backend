using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FullName,
    string University,
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterUserResponse>;