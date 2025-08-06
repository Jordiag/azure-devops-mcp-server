using System.Net;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Core.Common;

public class AzureDevOpsActionResult<T>
{
    public bool IsSuccessful { get; }
    public T Value { get; }
    public string? ErrorMessage { get; }
    public bool HasValue { get; set; }

    private AzureDevOpsActionResult(bool isSuccess, T value, string? errorMessage)
    {
        if (!isSuccess && value is not null)
        {
            Value = default!;
        }

        if (isSuccess && value is null)
        {
            throw new ArgumentNullException(nameof(value), "Success result must contain a non-null value.");
        }

        IsSuccessful = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static AzureDevOpsActionResult<T> Success(T value, ILogger? logger = null)
    {
        logger?.LogInformation("Azure DevOps action completed successfully.");
        return new(true, value, null);
    }

    public static AzureDevOpsActionResult<T> Failure(HttpStatusCode statusCode, string? errorMessage = null, ILogger? logger = null)
    {
        logger?.LogError("Request failed with status code {StatusCode}. {ErrorMessage}", (int)statusCode, errorMessage);
        return new(false, default!, $"http response status code: {(int)statusCode}, errorMessage: {errorMessage}");
    }

    public static AzureDevOpsActionResult<T> Failure(Exception exception, ILogger? logger = null)
    {
        logger?.LogError(exception, "Request failed with an exception.");
        return new(false, default!, $"the request ended raising an error exception: {exception.DumpFullException()}");
    }

    public static AzureDevOpsActionResult<T> Failure(string errorMessage, ILogger? logger = null)
    {
        logger?.LogError("Request failed with error: {ErrorMessage}", errorMessage);
        return new(false, default!, errorMessage);
    }

    public T EnsureSuccess()
    {
        if(IsSuccessful)
            return Value;      // Value is guaranteed non-null on success.

        throw new InvalidOperationException(
            ErrorMessage ?? "Azure DevOps operation failed.");
    }
}