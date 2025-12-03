using AspNetCoreRateLimit;
using AuthService.API.Extensions;
using AuthService.Infrastructure.Extensions;

namespace AuthService.API;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRedisService(
            Configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string not found"));

        services.AddDatabaseConfiguration(Configuration);
        services.AddApplicationServices();
        services.AddHealthChecksConfiguration(Configuration);
        services.AddRateLimitingConfiguration(Configuration);
        services.AddJwtAuthentication(Configuration);
        services.AddSwaggerConfiguration();
        services.AddCorsConfiguration();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseMiddleware<AuthService.Infrastructure.Middleware.ExceptionMiddleware>();
        app.UseIpRateLimiting();

        app.ApplyDatabaseMigrations();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthCheckEndpoints();

        app.Logger.LogInformation("AuthService started successfully");
    }
}