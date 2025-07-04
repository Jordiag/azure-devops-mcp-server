using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools
{
    [McpServerToolType]
    public class EchoTool
    {
        [McpServerTool, Description("Echoes the message back to the client.")]
        public string Echo(string message) => $"hello {message}";
    }
}
