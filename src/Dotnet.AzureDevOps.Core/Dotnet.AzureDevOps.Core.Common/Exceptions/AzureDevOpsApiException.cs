namespace Dotnet.AzureDevOps.Core.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when Azure DevOps API operations fail.
    /// Provides additional context such as HTTP status codes and response bodies
    /// to help with debugging and error recovery.
    /// </summary>
    public class AzureDevOpsApiException : AzureDevOpsException
    {
        /// <summary>
        /// Gets the HTTP status code returned by the Azure DevOps API, if available.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Gets the response body returned by the Azure DevOps API, if available.
        /// </summary>
        public string? ResponseBody { get; }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsApiException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="statusCode">The HTTP status code returned by the API.</param>
        /// <param name="responseBody">The response body returned by the API.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsApiException(string message, int? statusCode = null, string? responseBody = null, string? operationContext = null, string? correlationId = null)
            : base(message, operationContext, correlationId)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsApiException class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="statusCode">The HTTP status code returned by the API.</param>
        /// <param name="responseBody">The response body returned by the API.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsApiException(string message, Exception innerException, int? statusCode = null, string? responseBody = null, string? operationContext = null, string? correlationId = null)
            : base(message, innerException, operationContext, correlationId)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
