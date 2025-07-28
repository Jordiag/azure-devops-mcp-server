namespace Dotnet.AzureDevOps.Core.Overview.Options;

/// <summary>
/// Options to list pages from a wiki.
/// </summary>
public record WikiPagesBatchOptions
{
    public int Top { get; init; } = 20;

    public string? ContinuationToken { get; init; }

    public int? PageViewsForDays { get; init; }
}
