namespace Dotnet.AzureDevOps.Core.Common
{
    public class AzureDevOpsActionResult<T>
    {
        public bool Success { get; }

        public T? Value { get; }

        public string? Message { get; }

        public Exception? Exception { get; }

        private AzureDevOpsActionResult(bool success, T? value, string? message, Exception? exception)
        {
            Success = success;
            Value = value;
            Message = message;
            Exception = exception;
        }

        public static AzureDevOpsActionResult<T> FromValue(T value)
            => new AzureDevOpsActionResult<T>(true, value, null, null);

        public static AzureDevOpsActionResult<T> FromException(Exception exception, string? message = null)
            => new AzureDevOpsActionResult<T>(false, default, message ?? exception.Message, exception);
    }
}
