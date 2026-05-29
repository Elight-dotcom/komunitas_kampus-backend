using KomunitasKampus.API.Contracts.Auth;
using KomunitasKampus.API.Models;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Auth.Commands.Login;
using KomunitasKampus.Application.Features.Auth.Commands.Logout;
using KomunitasKampus.Application.Features.Auth.Commands.RefreshToken;
using KomunitasKampus.Application.Features.Auth.Commands.RegisterOrganization;
using KomunitasKampus.Application.Features.Auth.Commands.RegisterUser;
using KomunitasKampus.Application.Features.Auth.DTOs;
using KomunitasKampus.Application.Features.Auth.Queries.CheckUsernameAvailability;
using KomunitasKampus.Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KomunitasKampus.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        ISender sender,
        IOptions<JwtSettings> jwtOptions
    )
    {
        _sender = sender;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("register/organization")]
    [ProducesResponseType(typeof(ApiResponse<RegisterOrganizationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegisterOrganizationResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterOrganization(
        [FromBody] RegisterOrganizationRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var command = new RegisterOrganizationCommand(
                OrganizationName: request.OrganizationName,
                University: request.University,
                Username: request.Username,
                Email: request.Email,
                Password: request.Password,
                ConfirmPassword: request.ConfirmPassword
            );

            var response = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<RegisterOrganizationResponse>.Ok(
                response,
                "Registrasi organisasi berhasil."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<RegisterOrganizationResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
    }

    [HttpPost("register/user")]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var command = new RegisterUserCommand(
                FullName: request.FullName,
                University: request.University,
                Username: request.Username,
                Email: request.Email,
                Password: request.Password,
                ConfirmPassword: request.ConfirmPassword
            );

            var response = await _sender.Send(command, cancellationToken);

            return Ok(ApiResponse<RegisterUserResponse>.Ok(
                response,
                "Registrasi mahasiswa berhasil."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<RegisterUserResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
    }

    [HttpGet("username-availability")]
    [ProducesResponseType(typeof(ApiResponse<UsernameAvailabilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UsernameAvailabilityDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckUsernameAvailability(
        [FromQuery] string username,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await _sender.Send(
                new CheckUsernameAvailabilityQuery(username),
                cancellationToken
            );

            return Ok(ApiResponse<UsernameAvailabilityDto>.Ok(
                result,
                "Ketersediaan username berhasil dicek."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<UsernameAvailabilityDto>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var command = new LoginCommand(
                Identifier: request.Identifier,
                Password: request.Password
            );

            var result = await _sender.Send(command, cancellationToken);

            AppendRefreshTokenCookie(
                result.RefreshToken,
                result.RefreshTokenExpiresAt
            );

            return Ok(ApiResponse<LoginResponse>.Ok(
                result.Response,
                "Login berhasil."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<LoginResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
        catch (UnauthorizedAppException exception)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(
                exception.Message
            ));
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken
    )
    {
        try
        {
            var refreshToken = Request.Cookies[_jwtSettings.RefreshTokenCookieName];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(ApiResponse<LoginResponse>.Fail(
                    "Refresh token tidak ditemukan."
                ));
            }

            var command = new RefreshTokenCommand(refreshToken);

            var result = await _sender.Send(command, cancellationToken);

            AppendRefreshTokenCookie(
                result.RefreshToken,
                result.RefreshTokenExpiresAt
            );

            return Ok(ApiResponse<LoginResponse>.Ok(
                result.Response,
                "Token berhasil diperbarui."
            ));
        }
        catch (ValidationAppException exception)
        {
            return BadRequest(ApiResponse<LoginResponse>.Fail(
                exception.Message,
                exception.Errors
            ));
        }
        catch (UnauthorizedAppException exception)
        {
            DeleteRefreshTokenCookie();

            return Unauthorized(ApiResponse<LoginResponse>.Fail(
                exception.Message
            ));
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<LogoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken
    )
    {
        var response = await _sender.Send(new LogoutCommand(), cancellationToken);

        DeleteRefreshTokenCookie();

        return Ok(ApiResponse<LogoutResponse>.Ok(
            response,
            "Logout berhasil."
        ));
    }

    private void AppendRefreshTokenCookie(
        string refreshToken,
        DateTime expiresAt
    )
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _jwtSettings.CookieSecure,
            SameSite = ParseSameSiteMode(_jwtSettings.CookieSameSite),
            Expires = expiresAt,
            IsEssential = true,
            Path = "/"
        };

        Response.Cookies.Append(
            _jwtSettings.RefreshTokenCookieName,
            refreshToken,
            cookieOptions
        );
    }

    private void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _jwtSettings.CookieSecure,
            SameSite = ParseSameSiteMode(_jwtSettings.CookieSameSite),
            IsEssential = true,
            Path = "/"
        };

        Response.Cookies.Delete(
            _jwtSettings.RefreshTokenCookieName,
            cookieOptions
        );
    }

    private static SameSiteMode ParseSameSiteMode(string sameSite)
    {
        return sameSite.Trim().ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "lax" => SameSiteMode.Lax,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
    }
}