namespace Dotnet.AzureDevOps.Mcp.Server.DependencyInjection;

public class AzureDevOpsConfiguration
{
    public string SearchOrganizationUrl { get; set; } = string.Empty;
    public string OrganizationUrl { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty; 
    public string PersonalAccessToken { get; set; } = string.Empty;
}
