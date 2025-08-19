using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

[McpServerToolType]
public class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public string Echo(string message, ILogger? logger = null)
    {
        logger?.LogInformation("EchoTool: {Message}", message);
        return $"hello {message}";
    }
}
