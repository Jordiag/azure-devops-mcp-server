namespace Dotnet.AzureDevOps.Core.Common.Services
{
    /// <summary>
    /// Defines the type of operation being performed to determine appropriate retry strategies.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Read operations that are safe to retry (GET operations).
        /// </summary>
        Read = 0,

        /// <summary>
        /// Create operations that may cause duplicates if retried (POST operations).
        /// </summary>
        Create = 1,

        /// <summary>
        /// Update operations that may apply changes multiple times if retried (PUT/PATCH operations).
        /// </summary>
        Update = 2,

        /// <summary>
        /// Delete operations that may have inconsistent state if retried (DELETE operations).
        /// </summary>
        Delete = 3
    }
}
