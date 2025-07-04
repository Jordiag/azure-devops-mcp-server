namespace Dotnet.AzureDevOps.Core.Artifacts.Options;

/// <summary>Optional fields for updating an existing feed.</summary>
public record FeedUpdateOptions
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}