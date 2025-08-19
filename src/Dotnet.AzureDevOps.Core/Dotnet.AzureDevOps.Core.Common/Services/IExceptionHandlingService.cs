using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Core.Common.Services
{
    /// <summary>
    /// Service that provides centralized exception handling and transformation logic.
    /// Converts various types of exceptions into appropriate AzureDevOps-specific exceptions
    /// with enhanced context and error information.
    /// </summary>
    public interface IExceptionHandlingService
    {
        /// <summary>
        /// Transforms a generic exception into an appropriate AzureDevOps-specific exception.
        /// </summary>
        /// <param name="ex">The original exception.</param>
        /// <param name="operationName">The name of the operation that failed.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <returns>An AzureDevOps-specific exception with enhanced context.</returns>
        AzureDevOpsException TransformException(Exception ex, string operationName, string? correlationId = null);

        /// <summary>
        /// Logs an exception with appropriate log level and structured information.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="operationName">The name of the operation that failed.</param>
        /// <param name="correlationId">The correlation ID for tracking the operation.</param>
        /// <param name="logger">The logger to use for recording the exception.</param>
        void LogException(Exception ex, string operationName, string? correlationId, ILogger logger);
    }
}
