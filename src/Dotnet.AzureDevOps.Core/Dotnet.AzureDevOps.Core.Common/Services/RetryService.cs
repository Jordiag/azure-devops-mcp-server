using System.Net;
using System.Net.Sockets;
using Dotnet.AzureDevOps.Core.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace Dotnet.AzureDevOps.Core.Common.Services
{
    /// <summary>
    /// Service that provides retry logic for operations that may fail transiently.
    /// Uses Polly for sophisticated retry policies with exponential backoff,
    /// circuit breaking, and intelligent failure detection.
    /// </summary>
    public class RetryService : IRetryService
    {
        private readonly ILogger<RetryService> _logger;
        private readonly ResiliencePipeline _defaultPipeline;
        private readonly ResiliencePipeline<HttpResponseMessage> _httpPipeline;

        /// <summary>
        /// Initializes a new instance of the RetryService class.
        /// </summary>
        /// <param name="logger">The logger for capturing retry operations and failures.</param>
        public RetryService(ILogger<RetryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultPipeline = CreateDefaultPipeline();
            _httpPipeline = CreateHttpPipeline();
        }

        /// <inheritdoc />
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            Func<Exception, bool>? shouldRetry = null)
        {
            string operationId = Guid.NewGuid().ToString("N")[..8];

            ResilienceContext context = ResilienceContextPool.Shared.Get();
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationName"), operationName);
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationId"), operationId);

            try
            {
                return await _defaultPipeline.ExecuteAsync(async (ctx) =>
                {
                    try
                    {
                        return await operation();
                    }
                    catch(Exception ex) when(shouldRetry != null && !shouldRetry(ex))
                    {
                        throw new AzureDevOpsException($"Operation {operationName} failed: {ex.Message}", ex, operationName, operationId);
                    }
                }, context);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }

        /// <inheritdoc />
        public async Task ExecuteWithRetryAsync(
            Func<Task> operation,
            string operationName,
            Func<Exception, bool>? shouldRetry = null)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await operation();
                return true;
            }, operationName, shouldRetry);
        }

        /// <inheritdoc />
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            OperationType operationType,
            Func<Exception, bool>? shouldRetry = null,
            string? correlationId = null)
        {
            string operationId = correlationId ?? Guid.NewGuid().ToString("N")[..8];

            if(operationType != OperationType.Read && shouldRetry == null)
            {
                shouldRetry = ex => ShouldRetryNonIdempotentOperation(ex);
            }

            RetryStrategyOptions retryOptions = CreateRetryOptionsForOperationType(operationType, shouldRetry);
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(retryOptions)
                .AddTimeout(TimeSpan.FromMinutes(2))
                .Build();

            ResilienceContext context = ResilienceContextPool.Shared.Get();
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationName"), operationName);
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationId"), operationId);
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationType"), operationType.ToString());

            try
            {
                return await pipeline.ExecuteAsync(async (ctx) =>
                {
                    try
                    {
                        return await operation();
                    }
                    catch(Exception ex) when(shouldRetry != null && !shouldRetry(ex))
                    {
                        throw new AzureDevOpsException($"Operation {operationName} failed: {ex.Message}", ex, operationName, operationId);
                    }
                }, context);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }

        /// <summary>
        /// Executes HTTP operations with specialized HTTP retry policies.
        /// </summary>
        /// <param name="httpOperation">The HTTP operation to execute.</param>
        /// <param name="operationName">The name of the operation for logging.</param>
        /// <returns>The HTTP response message.</returns>
        public async Task<HttpResponseMessage> ExecuteHttpWithRetryAsync(
            Func<Task<HttpResponseMessage>> httpOperation,
            string operationName)
        {
            string operationId = Guid.NewGuid().ToString("N")[..8];

            ResilienceContext context = ResilienceContextPool.Shared.Get();
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationName"), operationName);
            context.Properties.Set(new ResiliencePropertyKey<string>("OperationId"), operationId);

            try
            {
                return await _httpPipeline.ExecuteAsync(async (ctx) =>
                {
                    return await httpOperation();
                }, context);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }

        /// <summary>
        /// Creates the default resilience pipeline for general operations.
        /// </summary>
        /// <returns>A configured resilience pipeline.</returns>
        private ResiliencePipeline CreateDefaultPipeline()
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>()
                                                        .Handle<SocketException>()
                                                        .Handle<TaskCanceledException>()
                                                        .Handle<TimeoutRejectedException>()
                                                        .Handle<VssServiceException>(ex => ShouldRetryVssException(ex))
                                                        .Handle<AzureDevOpsApiException>(ex => ShouldRetryApiException(ex)),
                    OnRetry = async args =>
                    {
                        string operationName = args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("OperationName"), out string? opName) ? opName ?? "Unknown" : "Unknown";
                        string operationId = args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("OperationId"), out string? opId) ? opId ?? "Unknown" : "Unknown";

                        _logger.LogWarning(args.Outcome.Exception,
                            "Operation {OperationName} failed (attempt {AttemptNumber}), retrying in {Delay}ms. OperationId: {OperationId}",
                            operationName, args.AttemptNumber, args.RetryDelay.TotalMilliseconds, operationId);

                        await Task.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromMinutes(2))
                .Build();
        }

        /// <summary>
        /// Creates a specialized resilience pipeline for HTTP operations.
        /// </summary>
        /// <returns>A configured HTTP resilience pipeline.</returns>
        private ResiliencePipeline<HttpResponseMessage> CreateHttpPipeline()
        {
            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                    .Handle<HttpRequestException>()
                                    .Handle<SocketException>()
                                    .Handle<TaskCanceledException>()
                                    .Handle<TimeoutRejectedException>()
                                    .HandleResult(response =>
                                        response.StatusCode >= HttpStatusCode.InternalServerError ||
                                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                                        response.StatusCode == HttpStatusCode.TooManyRequests),
                    OnRetry = async args =>
                    {
                        string operationName = args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("OperationName"), out string? opName) ? opName ?? "Unknown" : "Unknown";
                        string operationId = args.Context.Properties.TryGetValue(new ResiliencePropertyKey<string>("OperationId"), out string? opId) ? opId ?? "Unknown" : "Unknown";

                        _logger.LogWarning(args.Outcome.Exception,
                            "HTTP operation {OperationName} failed (attempt {AttemptNumber}), retrying in {Delay}ms. OperationId: {OperationId}",
                            operationName, args.AttemptNumber, args.RetryDelay.TotalMilliseconds, operationId);

                        await Task.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromMinutes(2))
                .Build();
        }

        /// <summary>
        /// Determines if a VSS service exception should be retried.
        /// </summary>
        /// <param name="ex">The VSS service exception.</param>
        /// <returns>True if the request should be retried, false otherwise.</returns>
        private static bool ShouldRetryVssException(VssServiceException ex)
        {
            // VssServiceException doesn't expose HttpStatusCode directly
            // We'll check the message or exception type for retry-worthy scenarios
            string message = ex.Message.ToLowerInvariant();

            return message.Contains("timeout") ||
                   message.Contains("connection") ||
                   message.Contains("network") ||
                   message.Contains("temporarily unavailable") ||
                   message.Contains("service unavailable") ||
                   message.Contains("internal server error") ||
                   message.Contains("bad gateway") ||
                   message.Contains("gateway timeout");
        }

        /// <summary>
        /// Determines if an Azure DevOps API exception should be retried.
        /// </summary>
        /// <param name="ex">The Azure DevOps API exception.</param>
        /// <returns>True if the request should be retried, false otherwise.</returns>
        private static bool ShouldRetryApiException(AzureDevOpsApiException ex) => ex.StatusCode switch
        {
            >= 500 => true, // Server errors
            429 => true,    // Rate limiting
            408 => true,    // Request timeout
            _ => false
        };

        /// <summary>
        /// Determines if an exception should be retried for non-idempotent operations.
        /// Only retries on clear network/infrastructure failures, not business logic errors.
        /// </summary>
        /// <param name="ex">The exception to evaluate.</param>
        /// <returns>True if the operation should be retried, false otherwise.</returns>
        private static bool ShouldRetryNonIdempotentOperation(Exception ex) =>
            ex switch
            {
                HttpRequestException httpEx when IsNetworkError(httpEx) => true,
                TaskCanceledException => true,
                SocketException => true,
                AzureDevOpsApiException apiEx when apiEx.StatusCode >= 500 => true, // Server errors only
                VssServiceException vssEx when IsServerErrorVssException(vssEx) => true,

                AzureDevOpsApiException apiEx when apiEx.StatusCode == 409 => false, // Conflict - resource exists
                AzureDevOpsApiException apiEx when apiEx.StatusCode == 400 => false, // Bad Request
                VssServiceException vssEx when IsClientErrorVssException(vssEx) => false,

                _ => false
            };

        /// <summary>
        /// Creates retry options based on the operation type.
        /// </summary>
        /// <param name="operationType">The type of operation being performed.</param>
        /// <param name="shouldRetry">Custom retry predicate.</param>
        /// <returns>Configured retry strategy options.</returns>
        private RetryStrategyOptions CreateRetryOptionsForOperationType(
            OperationType operationType,
            Func<Exception, bool>? shouldRetry) =>
            operationType switch
            {
                OperationType.Read => CreateReadRetryOptions(),
                _ => CreateWriteRetryOptions(shouldRetry)
            };

        /// <summary>
        /// Creates retry options optimized for read operations.
        /// </summary>
        /// <returns>Configured retry strategy options for read operations.</returns>
        private RetryStrategyOptions CreateReadRetryOptions() =>
            new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<SocketException>()
                        .Handle<TaskCanceledException>()
                        .Handle<TimeoutRejectedException>()
                        .Handle<VssServiceException>(ex => ShouldRetryVssException(ex))
                        .Handle<AzureDevOpsApiException>(ex => ShouldRetryApiException(ex)),
                OnRetry = CreateOnRetryCallback()
            };

        /// <summary>
        /// Creates retry options optimized for write operations (Create/Update/Delete).
        /// </summary>
        /// <param name="shouldRetry">Custom retry predicate.</param>
        /// <returns>Configured retry strategy options for write operations.</returns>
        private RetryStrategyOptions CreateWriteRetryOptions(Func<Exception, bool>? shouldRetry) =>
            new()
            {
                MaxRetryAttempts = 2, // Fewer retries
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Linear, // No exponential backoff
                UseJitter = false,
                ShouldHandle = new PredicateBuilder()
                    .Handle<Exception>(shouldRetry ?? (_ => false)),
                OnRetry = CreateOnRetryCallback()
            };

        /// <summary>
        /// Creates a callback function for retry events.
        /// </summary>
        /// <returns>Async callback for retry events.</returns>
        private Func<OnRetryArguments<object>, ValueTask> CreateOnRetryCallback() =>
            async args =>
            {
                (string operationName, string operationId, string operationType) = ExtractOperationProperties(args.Context);

                _logger.LogWarning(args.Outcome.Exception,
                    "Operation {OperationName} ({OperationType}) failed (attempt {AttemptNumber}), retrying in {Delay}ms. OperationId: {OperationId}",
                    operationName, operationType, args.AttemptNumber, args.RetryDelay.TotalMilliseconds, operationId);

                await Task.CompletedTask;
            };

        /// <summary>
        /// Extracts operation properties from the resilience context.
        /// </summary>
        /// <param name="context">The resilience context.</param>
        /// <returns>Tuple containing operation name, ID, and type.</returns>
        private static (string OperationName, string OperationId, string OperationType) ExtractOperationProperties(ResilienceContext context)
        {
            string operationName = context.Properties.TryGetValue(
                new ResiliencePropertyKey<string>("OperationName"), out string? opName) ? opName ?? "Unknown" : "Unknown";
            string operationId = context.Properties.TryGetValue(
                new ResiliencePropertyKey<string>("OperationId"), out string? opId) ? opId ?? "Unknown" : "Unknown";
            string operationType = context.Properties.TryGetValue(
                new ResiliencePropertyKey<string>("OperationType"), out string? opType) ? opType ?? "Unknown" : "Unknown";

            return (operationName, operationId, operationType);
        }

        /// <summary>
        /// Determines if an HTTP request exception is a network error.
        /// </summary>
        /// <param name="ex">The HTTP request exception.</param>
        /// <returns>True if it's a network error, false otherwise.</returns>
        private static bool IsNetworkError(HttpRequestException ex)
        {
            string message = ex.Message.ToLowerInvariant();
            return message.Contains("timeout") ||
                   message.Contains("connection") ||
                   message.Contains("network") ||
                   message.Contains("dns");
        }

        /// <summary>
        /// Determines if a VSS service exception represents a server error.
        /// </summary>
        /// <param name="ex">The VSS service exception.</param>
        /// <returns>True if it's a server error, false otherwise.</returns>
        private static bool IsServerErrorVssException(VssServiceException ex)
        {
            string message = ex.Message.ToLowerInvariant();
            return message.Contains("internal server error") ||
                   message.Contains("service unavailable") ||
                   message.Contains("bad gateway") ||
                   message.Contains("gateway timeout") ||
                   message.Contains("temporarily unavailable");
        }

        /// <summary>
        /// Determines if a VSS service exception represents a client error that shouldn't be retried.
        /// </summary>
        /// <param name="ex">The VSS service exception.</param>
        /// <returns>True if it's a client error, false otherwise.</returns>
        private static bool IsClientErrorVssException(VssServiceException ex)
        {
            string message = ex.Message.ToLowerInvariant();
            return message.Contains("conflict") ||
                   message.Contains("bad request") ||
                   message.Contains("already exists") ||
                   message.Contains("duplicate") ||
                   message.Contains("forbidden") ||
                   message.Contains("unauthorized");
        }
    }
}
