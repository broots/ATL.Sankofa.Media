using ATL.Sankofa.Media.Business.Configuration;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Services;
using ATL.Sankofa.Media.Data;
using ATL.Sankofa.Media.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATL.Sankofa.Media.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<MediaDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MediaDb")));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Cloudflare Stream
        services.Configure<CloudflareStreamSettings>(
            configuration.GetSection(CloudflareStreamSettings.SectionName));

        services.AddHttpClient<ICloudflareStreamClient, CloudflareStreamClient>();

        // JWT Settings
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // Business Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<ILiveStreamService, LiveStreamService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPaywallService, PaywallService>();
        services.AddScoped<IChannelService, ChannelService>();

        return services;
    }
}
