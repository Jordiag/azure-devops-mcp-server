namespace Dotnet.AzureDevOps.Core.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when configuration validation fails or configuration-related errors occur.
    /// This includes missing required configuration values, invalid formats, or configuration
    /// that cannot be processed correctly.
    /// </summary>
    public class AzureDevOpsConfigurationException : AzureDevOpsException
    {
        /// <summary>
        /// Gets the configuration key that caused the error, if applicable.
        /// </summary>
        public string? ConfigurationKey { get; }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsConfigurationException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="configurationKey">The configuration key that caused the error.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsConfigurationException(string message, string? configurationKey = null, string? operationContext = null, string? correlationId = null)
            : base(message, operationContext, correlationId) => 
            ConfigurationKey = configurationKey;

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsConfigurationException class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="configurationKey">The configuration key that caused the error.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsConfigurationException(string message, Exception innerException, string? configurationKey = null, string? operationContext = null, string? correlationId = null)
            : base(message, innerException, operationContext, correlationId) => 
            ConfigurationKey = configurationKey;
    }
}
