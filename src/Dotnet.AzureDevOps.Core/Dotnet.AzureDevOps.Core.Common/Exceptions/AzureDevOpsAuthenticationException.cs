namespace Dotnet.AzureDevOps.Core.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication or authorization fails with Azure DevOps services.
    /// This typically indicates issues with Personal Access Tokens, insufficient permissions,
    /// or expired credentials.
    /// </summary>
    public class AzureDevOpsAuthenticationException : AzureDevOpsException
    {
        /// <summary>
        /// Initializes a new instance of the AzureDevOpsAuthenticationException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsAuthenticationException(string message, string? operationContext = null, string? correlationId = null)
            : base(message, operationContext, correlationId) { }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsAuthenticationException class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsAuthenticationException(string message, Exception innerException, string? operationContext = null, string? correlationId = null)
            : base(message, innerException, operationContext, correlationId) { }
    }
}
