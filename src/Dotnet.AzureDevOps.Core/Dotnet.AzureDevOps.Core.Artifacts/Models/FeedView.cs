namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public enum FeedViewType
{
    None,
    Release,
    Implicit
}

public enum FeedVisibility
{
    Private,
    Collection,
    Organization,
    AadTenant
}

public record FeedView
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Url { get; init; }
    public FeedViewType Type { get; init; }
    public FeedVisibility Visibility { get; init; }
}

