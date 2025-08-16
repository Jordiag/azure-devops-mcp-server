using Dotnet.AzureDevOps.Mcp.Server;
using Dotnet.AzureDevOps.Mcp.Server.McpServer;
using Dotnet.AzureDevOps.Mcp.Server.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.ConfigureSettings();
builder.ConfigureLogging();

ValidateEnvironmentVariables(builder);

builder.ConfigureMcpServer();
builder.Services.AddMcpHealthChecks();

WebApplication app = builder.Build();

// Add security middleware early in the pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestSanitizationMiddleware>();

// Add HTTPS redirection for security
app.UseHttpsRedirection();

app.MapMcp("/mcp");
app.MapMcpHealthEndpoint();

await app.RunAsync();

public partial class Program
{
    protected Program() { }

    private static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        ILogger<Program> logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Validating environment variables...");

        string? organizationUrl = builder.Configuration["AZURE_DEVOPS_ORGANIZATION_URL"];
        if(string.IsNullOrWhiteSpace(organizationUrl))
        {
            logger.LogCritical("AZURE_DEVOPS_ORGANIZATION_URL environment variable is required and cannot be null or empty.");
            Environment.Exit(1);
        }

        string? projectName = builder.Configuration["AZURE_DEVOPS_PROJECT_NAME"];
        if(string.IsNullOrWhiteSpace(projectName))
        {
            logger.LogCritical("AZURE_DEVOPS_PROJECT_NAME environment variable is required and cannot be null or empty.");
            Environment.Exit(1);
        }

        string? pat = builder.Configuration["AZURE_DEVOPS_PAT"];
        if(string.IsNullOrWhiteSpace(pat))
        {
            logger.LogCritical("AZURE_DEVOPS_PAT environment variable is required and cannot be null or empty.");
            Environment.Exit(1);
        }

        if(pat.Length < 20)
        {
            logger.LogCritical("AZURE_DEVOPS_PAT appears to be too short to be a valid Personal Access Token.");
            Environment.Exit(1);
        }

        if(!Uri.TryCreate(organizationUrl, UriKind.Absolute, out _))
        {
            logger.LogCritical("AZURE_DEVOPS_ORGANIZATION_URL must be a valid URL format.");
            Environment.Exit(1);
        }

        // Log masked PAT for security audit
        IEncryptionService? encryptionService = builder.Services.BuildServiceProvider().GetService<IEncryptionService>();
        if(encryptionService != null)
        {
            logger.LogInformation("Using PAT: {MaskedPat}", encryptionService.MaskPersonalAccessToken(pat));
        }
    }
};