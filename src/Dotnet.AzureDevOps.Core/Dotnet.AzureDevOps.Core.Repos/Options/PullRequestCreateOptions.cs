namespace Dotnet.AzureDevOps.Core.Repos.Options;

/// <summary>
/// Encapsulates all required/optional fields for creating a Pull Request in Azure DevOps.
/// </summary>
public record PullRequestCreateOptions
{
    /// <summary>
    /// The name or ID of the repository in which to create the PR.
    /// Example: "MyRepo" or "abcd1234-..." GUID.
    /// </summary>
    public string RepositoryIdOrName { get; init; } = string.Empty;

    /// <summary>
    /// Pull request title (required).
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Optional detailed description of the PR.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The source branch. Must be in the form "refs/heads/feature/my-branch"
    /// </summary>
    public string SourceBranch { get; init; } = "refs/heads/feature/my-branch";

    /// <summary>
    /// The target branch. Must be in the form "refs/heads/main"
    /// </summary>
    public string TargetBranch { get; init; } = "refs/heads/main";

    /// <summary>
    /// (Optional) If you want to create a draft PR (not ready to merge).
    /// </summary>
    public bool IsDraft { get; init; } = false;

    // You could add a list of reviewers, labels, etc. here if needed.
    public string[]? Reviewers { get; init; }
}
