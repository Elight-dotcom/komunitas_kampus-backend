using System.Text;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Authentication;
using KomunitasKampus.Infrastructure.Persistence;
using KomunitasKampus.Infrastructure.Repositories;
using KomunitasKampus.Infrastructure.Caching;
using KomunitasKampus.Infrastructure.Realtime;
using StackExchange.Redis;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using KomunitasKampus.Infrastructure.HangfireJobs;
using KomunitasKampus.Infrastructure.Storage;
using Microsoft.Extensions.Options;
using Minio;
using KomunitasKampus.Infrastructure.Services;

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
        AddPostInfrastructure(services, configuration);
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IShareRepository, ShareRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<IStoryUploadService, StoryUploadService>();
        services.AddScoped<StoryExpirationJob>();
        services.AddScoped<IStoryBackgroundJobScheduler, StoryBackgroundJobScheduler>();
        services.AddScoped<IInteractionRealtimeNotifier, InteractionRealtimeNotifier>();

        services.AddSingleton<RedisCommentCacheService>();
        services.AddSingleton<RedisStoryCacheService>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConnectionString =
                configuration.GetConnectionString("Redis") ??
                configuration["Redis:ConnectionString"] ??
                "localhost:6379";

            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        var hangfireConnectionString =
            configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' belum dikonfigurasi."
            );

        services.AddHangfire(configurationBuilder =>
        {
            configurationBuilder.UseSimpleAssemblyNameTypeSerializer();
            configurationBuilder.UseRecommendedSerializerSettings();
            configurationBuilder.UsePostgreSqlStorage(hangfireConnectionString);
        });

        services.AddSignalR();

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

    private static void AddPostInfrastructure(
    IServiceCollection services,
    IConfiguration configuration
)
    {
        var minioSection = configuration.GetSection(MinioSettings.SectionName);

        services.Configure<MinioSettings>(minioSection);

        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<MinioSettings>>()
                .Value;

            if (string.IsNullOrWhiteSpace(settings.Endpoint))
            {
                throw new InvalidOperationException("MinIO Endpoint belum dikonfigurasi.");
            }

            if (string.IsNullOrWhiteSpace(settings.AccessKey))
            {
                throw new InvalidOperationException("MinIO AccessKey belum dikonfigurasi.");
            }

            if (string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                throw new InvalidOperationException("MinIO SecretKey belum dikonfigurasi.");
            }

            if (string.IsNullOrWhiteSpace(settings.BucketName))
            {
                throw new InvalidOperationException("MinIO BucketName belum dikonfigurasi.");
            }

            var clientBuilder = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (settings.UseSsl)
            {
                clientBuilder = clientBuilder.WithSSL();
            }

            return clientBuilder.Build();
        });

        services.AddScoped<IMinioService, MinioService>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<VideoTranscodeJob>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
    }
}