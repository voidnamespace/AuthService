using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Repositories;
using AuthService.Application.Services;
namespace AuthService.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}