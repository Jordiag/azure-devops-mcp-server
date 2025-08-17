using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;

namespace Dotnet.AzureDevOps.Core.Common.Services
{

    /// <summary>
    /// Default implementation of exception handling service.
    /// </summary>
    public class ExceptionHandlingService : IExceptionHandlingService
    {
        /// <inheritdoc />
        public AzureDevOpsException TransformException(Exception ex, string operationName, string? correlationId = null)
        {
            correlationId ??= Guid.NewGuid().ToString("N")[..8];

            return ex switch
            {
                AzureDevOpsException azureEx => azureEx, // Already transformed

                VssServiceException vssEx => TransformVssServiceException(vssEx, operationName, correlationId),

                HttpRequestException httpEx => TransformHttpRequestException(httpEx, operationName, correlationId),

                TaskCanceledException timeoutEx when timeoutEx.InnerException is TimeoutException =>
                    new AzureDevOpsApiException(
                        $"Request timeout for operation: {operationName}",
                        timeoutEx,
                        408,
                        operationContext: operationName,
                        correlationId: correlationId),

                JsonException jsonEx =>
                    new AzureDevOpsApiException(
                        $"Failed to parse JSON response for {operationName}: {jsonEx.Message}",
                        jsonEx,
                        operationContext: operationName,
                        correlationId: correlationId),

                ArgumentException argEx =>
                    new AzureDevOpsConfigurationException(
                        $"Invalid argument for {operationName}: {argEx.Message}",
                        argEx,
                        argEx.ParamName,
                        operationName,
                        correlationId),

                UnauthorizedAccessException =>
                    new AzureDevOpsAuthenticationException(
                        $"Access denied for operation: {operationName}",
                        operationName,
                        correlationId),

                _ => new AzureDevOpsException($"Unexpected error in {operationName}: {ex.Message}", ex, operationName, correlationId)
            };
        }

        /// <inheritdoc />
        public void LogException(Exception ex, string operationName, string? correlationId, ILogger logger)
        {
            correlationId ??= Guid.NewGuid().ToString("N")[..8];

            LogLevel logLevel = DetermineLogLevel(ex);

            // Use a constant message template for all logger calls to fix CA2254
            const string messageTemplate = "Operation {OperationName} failed. CorrelationId: {CorrelationId}{Context}{ResponseBody}";

            if (ex is AzureDevOpsException azureEx)
            {
                string context = azureEx.OperationContext != null ? $", Context: {azureEx.OperationContext}" : string.Empty;
                string responseBody = (azureEx is AzureDevOpsApiException apiEx && !string.IsNullOrEmpty(apiEx.ResponseBody))
                    ? $", ResponseBody: {apiEx.ResponseBody}"
                    : string.Empty;

                logger.Log(logLevel, ex, messageTemplate,
                    operationName, correlationId, context, responseBody);
            }
            else
            {
                logger.Log(logLevel, ex, messageTemplate, operationName, correlationId, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Transforms a VssServiceException into an appropriate AzureDevOps exception.
        /// </summary>
        /// <param name="vssEx">The VSS service exception.</param>
        /// <param name="operationName">The name of the operation that failed.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <returns>An appropriate AzureDevOps exception.</returns>
        private static AzureDevOpsException TransformVssServiceException(VssServiceException vssEx, string operationName, string correlationId)
        {
            // VssServiceException doesn't expose HttpStatusCode directly, analyze the message
            string message = vssEx.Message.ToLowerInvariant();

            if(message.Contains("unauthorized") || message.Contains("access denied"))
            {
                return new AzureDevOpsAuthenticationException(
                    $"Authentication failed for {operationName}: {vssEx.Message}",
                    vssEx,
                    operationName,
                    correlationId);
            }

            if(message.Contains("forbidden") || message.Contains("permission"))
            {
                return new AzureDevOpsAuthenticationException(
                    $"Access denied for {operationName}: {vssEx.Message}",
                    vssEx,
                    operationName,
                    correlationId);
            }

            if(message.Contains("not found") || message.Contains("does not exist"))
            {
                return new AzureDevOpsResourceNotFoundException(
                    $"Resource not found for {operationName}: {vssEx.Message}",
                    vssEx,
                    operationContext: operationName,
                    correlationId: correlationId);
            }

            return new AzureDevOpsApiException(
                $"Azure DevOps service error for {operationName}: {vssEx.Message}",
                vssEx,
                operationContext: operationName,
                correlationId: correlationId);
        }

        /// <summary>
        /// Transforms an HttpRequestException into an appropriate AzureDevOps exception.
        /// </summary>
        /// <param name="httpEx">The HTTP request exception.</param>
        /// <param name="operationName">The name of the operation that failed.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <returns>An appropriate AzureDevOps exception.</returns>
        private static AzureDevOpsException TransformHttpRequestException(HttpRequestException httpEx, string operationName, string correlationId)
        {
            string message = httpEx.Message.ToLowerInvariant();

            if(message.Contains("unauthorized"))
            {
                return new AzureDevOpsAuthenticationException(
                    $"Authentication failed for {operationName}: Please check your Personal Access Token",
                    httpEx,
                    operationName,
                    correlationId);
            }

            if(message.Contains("forbidden"))
            {
                return new AzureDevOpsAuthenticationException(
                    $"Access denied for {operationName}: Please check your permissions",
                    httpEx,
                    operationName,
                    correlationId);
            }

            if(message.Contains("not found") || message.Contains("404"))
            {
                return new AzureDevOpsResourceNotFoundException(
                    $"Resource not found for {operationName}",
                    httpEx,
                    operationContext: operationName,
                    correlationId: correlationId);
            }

            return new AzureDevOpsApiException(
                $"HTTP request failed for {operationName}: {httpEx.Message}",
                httpEx,
                operationContext: operationName,
                correlationId: correlationId);
        }

        /// <summary>
        /// Determines the appropriate log level for an exception.
        /// </summary>
        /// <param name="ex">The exception to evaluate.</param>
        /// <returns>The appropriate log level.</returns>
        private static LogLevel DetermineLogLevel(Exception ex)
        {
            return ex switch
            {
                AzureDevOpsAuthenticationException => LogLevel.Warning,
                AzureDevOpsResourceNotFoundException => LogLevel.Information,
                AzureDevOpsConfigurationException => LogLevel.Error,
                AzureDevOpsApiException apiEx when apiEx.StatusCode >= 500 => LogLevel.Error,
                AzureDevOpsApiException => LogLevel.Warning,
                _ => LogLevel.Error
            };
        }
    }
}
