namespace Dotnet.AzureDevOps.Core.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found in Azure DevOps.
    /// This includes projects, repositories, work items, builds, releases, and other entities
    /// that could not be located with the provided identifiers.
    /// </summary>
    public class AzureDevOpsResourceNotFoundException : AzureDevOpsException
    {
        /// <summary>
        /// Gets the type of resource that was not found (e.g., "Project", "Repository", "WorkItem").
        /// </summary>
        public string? ResourceType { get; }

        /// <summary>
        /// Gets the identifier of the resource that was not found.
        /// </summary>
        public string? ResourceId { get; }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsResourceNotFoundException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="resourceType">The type of resource that was not found.</param>
        /// <param name="resourceId">The identifier of the resource that was not found.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsResourceNotFoundException(string message, string? resourceType = null, string? resourceId = null, string? operationContext = null, string? correlationId = null)
            : base(message, operationContext, correlationId)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        /// <summary>
        /// Initializes a new instance of the AzureDevOpsResourceNotFoundException class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="resourceType">The type of resource that was not found.</param>
        /// <param name="resourceId">The identifier of the resource that was not found.</param>
        /// <param name="operationContext">The context in which the operation was being performed.</param>
        /// <param name="correlationId">The correlation ID for tracking related operations.</param>
        public AzureDevOpsResourceNotFoundException(string message, Exception innerException, string? resourceType = null, string? resourceId = null, string? operationContext = null, string? correlationId = null)
            : base(message, innerException, operationContext, correlationId)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }
}
