using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;

namespace AuthService.Application.Extensions;

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
