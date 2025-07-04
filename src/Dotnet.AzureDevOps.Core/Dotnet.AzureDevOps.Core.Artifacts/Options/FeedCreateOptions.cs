namespace Dotnet.AzureDevOps.Core.Artifacts.Options;

/// <summary>Parameters required to create a new feed.</summary>
public record FeedCreateOptions
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
