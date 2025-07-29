namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record FeedView
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Url { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Visibility { get; init; } = string.Empty;
}

