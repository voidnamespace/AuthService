using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using AuthService.Application.Common.Interfaces;
using AuthService.Infrastructure.Services;

namespace AuthService.Infrastructure.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisService(this IServiceCollection services, string configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration));

        services.AddSingleton<IRedisService, RedisService>();

        return services;
    }
}
