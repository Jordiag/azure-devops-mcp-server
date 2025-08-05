using System;
using System.ComponentModel;
using System.Net;
using Dotnet.AzureDevOps.Core.Common;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

[McpServerToolType]
public static class CommonTools
{
    [McpServerTool, Description("Dumps an exception to a full string representation.")]
    public static string DumpFullException(Exception exception, bool includeData = true)
        => exception.DumpFullException(includeData);

    [McpServerTool, Description("Creates a failure result from an HTTP status code.")]
    public static AzureDevOpsActionResult<string> FailureFromStatusCode(int statusCode, string? errorMessage = null)
        => AzureDevOpsActionResult<string>.Failure((HttpStatusCode)statusCode, errorMessage);

    [McpServerTool, Description("Creates a failure result from an exception message.")]
    public static AzureDevOpsActionResult<string> FailureFromException(string message)
        => AzureDevOpsActionResult<string>.Failure(new Exception(message));

    [McpServerTool, Description("Creates a failure result from an error message.")]
    public static AzureDevOpsActionResult<string> FailureFromMessage(string errorMessage)
        => AzureDevOpsActionResult<string>.Failure(errorMessage);
}
