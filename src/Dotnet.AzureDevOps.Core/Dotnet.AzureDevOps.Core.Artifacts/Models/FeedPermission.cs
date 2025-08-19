namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record FeedPermission
{
    public FeedRole Role { get; init; }
    public string? IdentityDescriptor { get; init; }
    public string? IdentityId { get; init; }
    public string? DisplayName { get; init; }
    public bool IsInheritedRole { get; init; }
}

