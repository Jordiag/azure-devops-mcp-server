using System.Net;

namespace Dotnet.AzureDevOps.Core.Common;

public class AzureDevOpsActionResult<T>
{
    public bool IsSuccessful { get; }
    public T Value { get; }
    public string? ErrorMessage { get; }
    public bool HasValue { get; set; }

    private AzureDevOpsActionResult(bool isSuccess, T value, string? errorMessage)
    {
        if(!isSuccess && value is not null)
        {
            Value = default!;
        }

        if(isSuccess && value is null)
        {
            throw new ArgumentNullException(nameof(value), "Success result must contain a non-null value.");
        }

        IsSuccessful = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static AzureDevOpsActionResult<T> Success(T value)
        => new(true, value, null);

    public static AzureDevOpsActionResult<T> Failure(HttpStatusCode statusCode, string? errorMessage = null)
        => new(false, default!, $"http response status code: {(int)statusCode}, errorMessage: {errorMessage}");

    public static AzureDevOpsActionResult<T> Failure(Exception exception)
        => new(false, default!, $"the request ended raising an error exception: {exception.DumpFullException()}");

    public static AzureDevOpsActionResult<T> Failure(string errorMessage)
        => new(false, default!, errorMessage);
}