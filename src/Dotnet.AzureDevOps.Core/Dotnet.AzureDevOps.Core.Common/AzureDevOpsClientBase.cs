using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Common
{
    /// <summary>
    /// Base class for Azure DevOps client implementations providing common functionality
    /// for connection management, disposal patterns, and shared dependencies.
    /// Eliminates code duplication across client classes while ensuring consistent
    /// connection handling and resource management patterns.
    /// </summary>
    public abstract class AzureDevOpsClientBase : IDisposable, IAsyncDisposable
    {
        protected readonly VssConnection Connection;
        protected readonly ILogger Logger;
        protected readonly string ProjectName;
        protected readonly string OrganizationUrl;
        protected readonly IRetryService RetryService;
        protected readonly IExceptionHandlingService ExceptionHandlingService;
        protected bool Disposed;

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsClientBase class with common Azure DevOps connection setup.
        /// </summary>
        /// <param name="organizationUrl">The Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal Access Token for authentication</param>
        /// <param name="projectName">Optional project name for project-scoped operations</param>
        /// <param name="logger">Optional logger instance</param>
        /// <param name="retryService">Retry service for handling transient failures</param>
        /// <param name="exceptionHandlingService">Exception handling service for transforming exceptions</param>
        protected AzureDevOpsClientBase(
            string organizationUrl,
            string personalAccessToken,
            string? projectName = null,
            ILogger? logger = null,
            IRetryService? retryService = null,
            IExceptionHandlingService? exceptionHandlingService = null)
        {
            ValidateConstructorArguments(organizationUrl, personalAccessToken);

            OrganizationUrl = organizationUrl;
            ProjectName = projectName ?? string.Empty;
            Logger = logger ?? NullLogger.Instance;

            RetryService = retryService ?? CreateDefaultRetryService();
            ExceptionHandlingService = exceptionHandlingService ?? CreateDefaultExceptionHandlingService();

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            Connection = new VssConnection(new Uri(organizationUrl), credentials);
        }

        /// <summary>
        /// Executes an operation with enhanced exception handling and retry logic.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">A descriptive name for the operation.</param>
        /// <param name="correlationId">Optional correlation ID for tracking related operations.</param>
        /// <returns>The result of the operation.</returns>
        protected async Task<T> ExecuteWithExceptionHandlingAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            string? correlationId = null)
        {
            correlationId ??= Guid.NewGuid().ToString("N")[..8];

            try
            {
                Logger.LogDebug("Starting operation {OperationName}. CorrelationId: {CorrelationId}",
                    operationName, correlationId);

                T result = await RetryService.ExecuteWithRetryAsync(operation, operationName);

                Logger.LogDebug("Operation {OperationName} completed successfully. CorrelationId: {CorrelationId}",
                    operationName, correlationId);

                return result;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Operation {OperationName} failed with correlation ID {CorrelationId}",
                    operationName, correlationId);

                AzureDevOpsException transformedException = ExceptionHandlingService.TransformException(ex, operationName, correlationId);
                ExceptionHandlingService.LogException(transformedException, operationName, correlationId, Logger);
                throw transformedException;
            }
        }

        /// <summary>
        /// Executes an operation with enhanced exception handling and operation-specific retry logic.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">A descriptive name for the operation.</param>
        /// <param name="operationType">The type of operation to determine appropriate retry strategy.</param>
        /// <param name="correlationId">Optional correlation ID for tracking related operations.</param>
        /// <returns>The result of the operation.</returns>
        protected async Task<T> ExecuteWithExceptionHandlingAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            OperationType operationType,
            string? correlationId = null)
        {
            correlationId ??= Guid.NewGuid().ToString("N")[..8];

            try
            {
                Logger.LogDebug("Starting {OperationType} operation {OperationName}. CorrelationId: {CorrelationId}",
                    operationType, operationName, correlationId);

                T result = await RetryService.ExecuteWithRetryAsync(operation, operationName, operationType, correlationId: correlationId);

                Logger.LogDebug("Operation {OperationName} completed successfully. CorrelationId: {CorrelationId}",
                    operationName, correlationId);

                return result;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "{OperationType} operation {OperationName} failed with correlation ID {CorrelationId}",
                    operationType, operationName, correlationId);

                AzureDevOpsException transformedException = ExceptionHandlingService.TransformException(ex, operationName, correlationId);
                ExceptionHandlingService.LogException(transformedException, operationName, correlationId, Logger);
                throw transformedException;
            }
        }

        /// <summary>
        /// Executes an operation with enhanced exception handling and retry logic.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">A descriptive name for the operation.</param>
        /// <param name="correlationId">Optional correlation ID for tracking related operations.</param>
        /// <returns>A task representing the completion of the operation.</returns>
        protected async Task ExecuteWithExceptionHandlingAsync(
            Func<Task> operation,
            string operationName,
            string? correlationId = null) =>
            await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    await operation();
                    return true;
                }, operationName, correlationId);

        /// <summary>
        /// Validates constructor arguments and throws appropriate exceptions for invalid values.
        /// Skips validation for placeholder values used in integration tests.
        /// </summary>
        /// <param name="organizationUrl">The organization URL to validate.</param>
        /// <param name="personalAccessToken">The personal access token to validate.</param>
        private static void ValidateConstructorArguments(string organizationUrl, string personalAccessToken)
        {
            if(organizationUrl == "https://dev.azure.com/placeholder" &&
                personalAccessToken == "placeholder-token")
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(organizationUrl))
            {
                throw new AzureDevOpsConfigurationException(
                    "Organization URL is required and cannot be null or empty",
                    "OrganizationUrl",
                    "ClientInitialization");
            }

            if(string.IsNullOrWhiteSpace(personalAccessToken))
            {
                throw new AzureDevOpsConfigurationException(
                    "Personal Access Token is required and cannot be null or empty",
                    "PersonalAccessToken",
                    "ClientInitialization");
            }

            if(!Uri.IsWellFormedUriString(organizationUrl, UriKind.Absolute))
            {
                throw new AzureDevOpsConfigurationException(
                    $"Organization URL '{organizationUrl}' is not a valid absolute URI",
                    "OrganizationUrl",
                    "ClientInitialization");
            }
        }

        /// <summary>
        /// Creates a default retry service instance when none is injected.
        /// Uses a NullLogger to avoid casting issues in tests.
        /// </summary>
        /// <returns>A new RetryService instance with appropriate logger.</returns>
        private static RetryService CreateDefaultRetryService()
        {
            ILogger<RetryService> retryServiceLogger = NullLogger<RetryService>.Instance;
            return new RetryService(retryServiceLogger);
        }

        /// <summary>
        /// Creates a default exception handling service instance when none is injected.
        /// </summary>
        /// <returns>A new ExceptionHandlingService instance.</returns>
        private static ExceptionHandlingService CreateDefaultExceptionHandlingService() =>
            new ExceptionHandlingService();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!Disposed)
            {
                if(disposing)
                {
                    Connection?.Dispose();
                }
                Disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            Connection?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
