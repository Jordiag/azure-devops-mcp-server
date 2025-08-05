namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public class IdentityRef
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string UniqueName { get; set; } = string.Empty;
}