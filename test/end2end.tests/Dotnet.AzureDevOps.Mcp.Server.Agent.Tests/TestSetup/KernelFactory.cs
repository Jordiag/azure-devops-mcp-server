using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
#pragma warning disable SKEXP0001

namespace Dotnet.AzureDevOps.Mcp.Server.Agent.Tests.TestSetup;

internal static class KernelFactory
{
    public static async Task<Kernel> ForMcpAsync(this Kernel kernel, Uri uri, HttpClient client, Func<McpClientTool, bool> predicate)
    {
        IMcpClient mcpClient = await McpClientFactory
            .CreateAsync(new SseClientTransport(new SseClientTransportOptions { Endpoint = uri }, client));

        IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

        kernel
            .Plugins
            .AddFromFunctions("McpTester", tools
                .Where(predicate)
                .Select(t => t.AsKernelFunction()));

        return kernel;
    }
}