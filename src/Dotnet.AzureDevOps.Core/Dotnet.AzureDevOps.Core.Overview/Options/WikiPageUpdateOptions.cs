namespace Dotnet.AzureDevOps.Core.Overview.Options;

/// <summary>
/// Parameters for creating or updating a wiki page.
/// </summary>
public record WikiPageUpdateOptions
{
    public string Path { get; init; } = "/";

    public string Content { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;
}
