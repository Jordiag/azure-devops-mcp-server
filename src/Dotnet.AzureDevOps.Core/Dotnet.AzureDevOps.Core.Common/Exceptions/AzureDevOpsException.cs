namespace Dotnet.AzureDevOps.Core.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all Azure DevOps related errors.
    /// Provides common properties for operation context and correlation tracking.
    /// </summary>
    public class AzureDevOpsException : Exception
    {
        /// <summary>
        /// Gets the context in which the operation was being performed when the exception occurred.
        /// </summary>
        public string? OperationContext { get; }

        /// <summary>
        /// Gets the correlation ID that can be used to track related operations and errors.
        /// </summary>
        public string? CorrelationId { get; }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsException(string message, string? operationContext = null, string? correlationId = null)
            : base(message)
        {
            OperationContext = operationContext;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsException class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsException(string message, Exception innerException, string? operationContext = null, string? correlationId = null)
            : base(message, innerException)
        {
            OperationContext = operationContext;
            CorrelationId = correlationId;
        }
    }
}
