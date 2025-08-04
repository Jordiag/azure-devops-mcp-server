using System.Net;

namespace Dotnet.AzureDevOps.Core.Common;

public class AzureDevOpsActionResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public HttpStatusCode? StatusCode { get; }
    public string? ErrorMessage { get; }

    private AzureDevOpsActionResult(bool isSuccess, T? value, HttpStatusCode? statusCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public static AzureDevOpsActionResult<T> Success(T value) => new(true, value, null, null);

    public static AzureDevOpsActionResult<T> Failure(HttpStatusCode statusCode, string? errorMessage = null)
        => new(false, default, statusCode, errorMessage);

    public static AzureDevOpsActionResult<T> Failure(Exception exception)
        => new(false, default, null, exception.DumpFullException());

    public static AzureDevOpsActionResult<T> Failure(string errorMessage)
        => new(false, default, null, errorMessage);
}
