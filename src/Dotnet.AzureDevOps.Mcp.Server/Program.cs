using Dotnet.AzureDevOps.Mcp.Server;
using Dotnet.AzureDevOps.Mcp.Server.McpServer;
using Microsoft.AspNetCore.Builder;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.ConfigureSettings();
builder.ConfigureLogging();
builder.ConfigureMcpServer();
builder.Services.AddMcpHealthChecks();

WebApplication app = builder.Build();

app.MapMcp();
app.MapMcpHealthEndpoint();

await app.RunAsync();

public partial class Program 
{
    protected Program() { }
};