namespace Dotnet.AzureDevOps.Core.Common.Services
{
    /// <summary>
    /// Service interface for executing operations with retry logic.
    /// </summary>
    public interface IRetryService
    {
        /// <summary>
        /// Executes an operation with automatic retry logic for transient failures.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">A descriptive name for the operation for logging purposes.</param>
        /// <param name="shouldRetry">Optional custom logic to determine if a retry should be attempted.</param>
        /// <returns>The result of the operation.</returns>
        Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            Func<Exception, bool>? shouldRetry = null);

        /// <summary>
        /// Executes an operation with automatic retry logic for transient failures.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">A descriptive name for the operation for logging purposes.</param>
        /// <param name="operationType">The type of operation to determine retry strategy.</param>
        /// <param name="shouldRetry">Optional custom logic to determine if a retry should be attempted.</param>
        /// <param name="correlationId">Optional correlation ID for tracking the operation.</param>
        /// <returns>The result of the operation.</returns>
        Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            OperationType operationType,
            Func<Exception, bool>? shouldRetry = null,
            string? correlationId = null);

        /// <summary>
        /// Executes an operation with automatic retry logic for transient failures.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="operationName">A descriptive name for the operation for logging purposes.</param>
        /// <param name="shouldRetry">Optional custom logic to determine if a retry should be attempted.</param>
        /// <returns>A task representing the completion of the operation.</returns>
        Task ExecuteWithRetryAsync(
            Func<Task> operation,
            string operationName,
            Func<Exception, bool>? shouldRetry = null);

        /// <summary>
        /// Executes HTTP operations with specialized HTTP retry policies.
        /// </summary>
        /// <param name="httpOperation">The HTTP operation to execute.</param>
        /// <param name="operationName">The name of the operation for logging.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> ExecuteHttpWithRetryAsync(
            Func<Task<HttpResponseMessage>> httpOperation,
            string operationName);
    }
}
