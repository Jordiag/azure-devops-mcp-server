using Dotnet.AzureDevOps.Mcp.Server.McpServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Dotnet.AzureDevOps.Mcp.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.ConfigureSettings();
        builder.ConfigureLogging();
        builder.ConfigureMcpServer();
        builder.Services.AddMcpHealthChecks();

        WebApplication app = builder.Build();

        app.MapMcp();
        app.MapMcpHealthEndpoint();

        app.Run();
    }
}