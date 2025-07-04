using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dotnet.AzureDevOps.Mcp.Server;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers the standard HealthCheckService so we can query it later.
    /// </summary>
    public static IServiceCollection AddMcpHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    /// <summary>
    /// Minimal-API endpoint: GET /health
    /// - 200 OK  = Healthy
    /// - 503     = Degraded / Unhealthy
    /// </summary>
    public static WebApplication MapMcpHealthEndpoint(this WebApplication app)
    {
        app.MapGet("/health", async (HealthCheckService hc) =>
        {
            HealthReport report = await hc.CheckHealthAsync();

            return report.Status switch
            {
                HealthStatus.Healthy => Results.Ok("Healthy"),
                HealthStatus.Degraded => Results.StatusCode(503),
                _ => Results.StatusCode(503)
            };
        });

        return app;
    }
}
