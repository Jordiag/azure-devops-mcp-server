using Dotnet.AzureDevOps.Mcp.Server.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.AzureDevOps.Mcp.Server.DependencyInjection;

/// <summary>
/// Extension methods for registering security services in the dependency injection container.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Adds security services including input sanitization, encryption, and authorization.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IInputSanitizer, InputSanitizer>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        services.AddOptions<EncryptionOptions>()
            .BindConfiguration(EncryptionOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddOptions<SecurityHeadersOptions>()
            .BindConfiguration(SecurityHeadersOptions.SectionName)
            .ValidateDataAnnotations();

        return services;
    }

    /// <summary>
    /// Adds security middleware to the service collection.
    /// Note: Middleware registration is done in the application builder configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSecurityMiddleware(this IServiceCollection services)
    {
        services.AddScoped<SecurityHeadersMiddleware>();
        services.AddScoped<RequestSanitizationMiddleware>();

        return services;
    }
}