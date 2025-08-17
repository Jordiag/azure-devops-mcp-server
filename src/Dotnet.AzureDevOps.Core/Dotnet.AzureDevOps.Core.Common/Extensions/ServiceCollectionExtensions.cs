using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dotnet.AzureDevOps.Core.Common.Services;

namespace Dotnet.AzureDevOps.Core.Common.Extensions
{
    /// <summary>
    /// Extension methods for registering Azure DevOps core services with dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Azure DevOps core services to the service collection.
        /// Includes Polly-based retry service and exception handling service.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAzureDevOpsCoreServices(this IServiceCollection services)
        {
            // Register core exception handling and Polly-based retry services
            services.AddSingleton<IRetryService, RetryService>();
            services.AddSingleton<IExceptionHandlingService, ExceptionHandlingService>();

            return services;
        }

        /// <summary>
        /// Adds Azure DevOps core services to the service collection with custom logger.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="loggerFactory">The logger factory to use for creating loggers.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAzureDevOpsCoreServices(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            // Register core exception handling and Polly-based retry services with specific logger
            services.AddSingleton<IRetryService>(provider => 
                new RetryService(loggerFactory.CreateLogger<RetryService>()));
            services.AddSingleton<IExceptionHandlingService, ExceptionHandlingService>();

            return services;
        }
    }
}
