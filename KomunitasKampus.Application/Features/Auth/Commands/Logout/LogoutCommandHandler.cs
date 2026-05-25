using KomunitasKampus.Application.Features.Auth.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    public Task<LogoutResponse> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken
    )
    {
        var response = new LogoutResponse("Logout berhasil.");

        return Task.FromResult(response);
    }
}