using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos.Options;

/// <summary>
/// Simple filter options for listing PRs.  Extend as needed (date range, author, etc.).
/// </summary>
public record PullRequestSearchOptions
{
    public PullRequestStatus Status { get; init; } = PullRequestStatus.Active;

    public string? TargetBranch { get; init; }      // "refs/heads/main"

    public string? SourceBranch { get; init; }      // "refs/heads/feature/*"
}
