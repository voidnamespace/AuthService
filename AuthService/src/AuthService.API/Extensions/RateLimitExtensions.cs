using AspNetCoreRateLimit;

namespace AuthService.API.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimitingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Needed to store rate limit counters and ip rules
        services.AddMemoryCache();

        // Load general configuration from appsettings.json
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

        // Load ip rules from appsettings.json
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

        // Inject counter and rules stores
        services.AddInMemoryRateLimiting();

        // Configuration
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }
}