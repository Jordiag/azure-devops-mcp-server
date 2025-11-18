namespace Dotnet.AzureDevOps.Core.Repos.Options;

/// <summary>
/// Encapsulates all required/optional fields for creating a Pull Request in Azure DevOps.
/// </summary>
public record PullRequestCreateOptions
{
    /// <summary>
    /// The name or ID of the project in which to create the PR.
    /// Example: "MyProject" or "abcd1234-..." GUID.
    /// </summary>
    public required string ProjectIdOrName { get; set; }

    /// <summary>
    /// The name or ID of the repository in which to create the PR.
    /// Example: "MyRepo" or "abcd1234-..." GUID.
    /// </summary>
    public required string RepositoryIdOrName { get; set; }

    /// <summary>
    /// Pull request title (required).
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Optional detailed description of the PR.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The source branch. Must be in the form "refs/heads/feature/my-branch"
    /// </summary>
    public required string SourceBranch { get; set; }

    /// <summary>
    /// The target branch. Must be in the form "refs/heads/main"
    /// </summary>
    public required string TargetBranch { get; set; }

    /// <summary>
    /// (Optional) If you want to create a draft PR (not ready to merge).
    /// </summary>
    public required bool IsDraft { get; set; } 
}
