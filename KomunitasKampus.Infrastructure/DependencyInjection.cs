using System.Text;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Domain.Repositories;
using KomunitasKampus.Infrastructure.Authentication;
using KomunitasKampus.Infrastructure.Persistence;
using KomunitasKampus.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace KomunitasKampus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AddDatabase(services, configuration);
        AddAuthenticationServices(services, configuration);
        AddRepositories(services);

        return services;
    }

    private static void AddDatabase(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' belum dikonfigurasi."
            );
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });
    }

    private static void AddAuthenticationServices(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);

        var jwtSettings = jwtSection.Get<JwtSettings>();

        if (jwtSettings is null)
        {
            throw new InvalidOperationException(
                "Konfigurasi JWT belum tersedia di appsettings."
            );
        }

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            throw new InvalidOperationException(
                "JWT Secret belum dikonfigurasi."
            );
        }

        var secretKey = Encoding.UTF8.GetBytes(jwtSettings.Secret);

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT Secret minimal harus 32 karakter."
            );
        }

        services.Configure<JwtSettings>(jwtSection);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtSettings.CookieSecure;
                options.SaveToken = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddScoped<IAuthTokenService, JwtTokenService>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
    }
}