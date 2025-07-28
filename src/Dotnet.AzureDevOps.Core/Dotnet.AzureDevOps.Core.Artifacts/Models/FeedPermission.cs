namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public enum FeedRole
{
    Custom,
    None,
    Reader,
    Contributor,
    Administrator,
    Collaborator
}

public record FeedPermission
{
    public FeedRole Role { get; init; }
    public string? IdentityDescriptor { get; init; }
    public string? IdentityId { get; init; }
    public string? DisplayName { get; init; }
    public bool IsInheritedRole { get; init; }
}

