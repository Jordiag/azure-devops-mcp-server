using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

[McpServerToolType]
public class ReposTools(IReposClient reposClient, ILogger<ReposTools> logger)
{
    private readonly IReposClient _reposClient = reposClient;
    private readonly ILogger<ReposTools> _logger = logger;

    [McpServerTool, Description("Creates a new pull request in Azure DevOps to propose code changes from a source branch to a target branch. Requires repository name/ID, title, description, source branch, target branch, and optionally whether it's a draft. Returns the unique pull request ID. The pull request will be in Active status and available for review.")]
    public async Task<int> CreatePullRequestAsync(PullRequestCreateOptions options) =>
        (await _reposClient.CreatePullRequestAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a specific pull request from Azure DevOps, including its current status, source/target branches, reviewers, comments, and merge commit information. Useful for checking pull request progress or getting details before performing operations like completing or abandoning.")]
    public async Task<GitPullRequest> GetPullRequestAsync(string repositoryId, int pullRequestId) =>
        (await _reposClient.GetPullRequestAsync(repositoryId, pullRequestId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Completes (merges) an active pull request in Azure DevOps, integrating the source branch changes into the target branch. Options include squash merge (combines all commits into one), delete source branch after merge, and custom commit message. The pull request status changes to Completed after successful merge.")]
    public async Task<GitPullRequest> CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null) =>
        (await _reposClient.CompletePullRequestAsync(repositoryId, pullRequestId, squashMerge, deleteSourceBranch, lastMergeSourceCommit, commitMessage)).EnsureSuccess(_logger);

    [McpServerTool, Description("Abandons an active pull request in Azure DevOps without merging the changes. This closes the pull request and marks it as Abandoned. The source branch and its changes remain intact but are no longer proposed for merging. Can be reactivated later if needed.")]
    public async Task<GitPullRequest> AbandonPullRequestAsync(string repositoryId, int pullRequestId) =>
        (await _reposClient.AbandonPullRequestAsync(repositoryId, pullRequestId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves a list of pull requests from an Azure DevOps repository based on search criteria such as status (Active, Completed, Abandoned), creator, reviewer, source/target branches, and date ranges. Useful for finding pull requests that need attention or reviewing recent changes.")]
    public async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions options) =>
        (await _reposClient.ListPullRequestsAsync(repositoryId, options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new Git repository in the current Azure DevOps project. The repository will be empty initially and ready for code commits. Returns the unique repository GUID identifier that can be used for subsequent operations.")]
    public async Task<Guid> CreateRepositoryAsync(string repositoryName) =>
        (await _reposClient.CreateRepositoryAsync(repositoryName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a Git repository in Azure DevOps using its unique GUID identifier. Returns repository metadata including name, URL, default branch, size, and project information.")]
    public async Task<GitRepository> GetRepositoryAsync(Guid repositoryId) =>
        (await _reposClient.GetRepositoryAsync(repositoryId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves detailed information about a Git repository in Azure DevOps using its name. Returns repository metadata including GUID, URL, default branch, size, and project information. Useful when you know the repository name but need its ID for other operations.")]
    public async Task<GitRepository> GetRepositoryByNameAsync(string repositoryName) =>
        (await _reposClient.GetRepositoryByNameAsync(repositoryName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves a complete list of all Git repositories in the current Azure DevOps project. Returns repository metadata including names, GUIDs, URLs, default branches, and sizes. Useful for discovering available repositories or getting an overview of the project's code structure.")]
    public async Task<IReadOnlyList<GitRepository>> ListRepositoriesAsync() =>
        (await _reposClient.ListRepositoriesAsync()).EnsureSuccess(_logger);

    [McpServerTool, Description("Permanently deletes a Git repository from Azure DevOps using its GUID identifier. This action cannot be undone and will remove all code, branches, tags, pull request history, and repository settings. Returns true if deletion was successful.")]
    public async Task<bool> DeleteRepositoryAsync(Guid repositoryId) =>
        (await _reposClient.DeleteRepositoryAsync(repositoryId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves all branches from an Azure DevOps Git repository. Returns branch information including names, commit SHAs, and whether they are the default branch. Useful for understanding the repository's branch structure before creating pull requests or new branches.")]
    public async Task<IReadOnlyList<GitRef>> ListBranchesAsync(string repositoryId) => 
        (await _reposClient.ListBranchesAsync(repositoryId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a new branch in an Azure DevOps Git repository based on a specific commit SHA. The new branch will point to the specified commit and can be used for feature development or pull requests. Returns the result of the branch creation operation including success status.")]
    public async Task<List<GitRefUpdateResult>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha) =>
        (await _reposClient.CreateBranchAsync(repositoryId, newRefName, baseCommitSha)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves information about a specific branch from an Azure DevOps Git repository, including its current commit SHA, name, and metadata. Useful for checking if a branch exists and getting its current state before creating pull requests or performing branch operations.")]
    public async Task<GitRef> GetBranchAsync(string repositoryId, string branchName) =>
        (await _reposClient.GetBranchAsync(repositoryId, branchName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Searches for commits in an Azure DevOps Git repository based on specified criteria such as author, date range, commit message, or file paths. Returns commit information including SHA, author, date, message, and changed files. Useful for finding specific commits or analyzing repository history.")]
    public async Task<IReadOnlyList<GitCommitRef>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100) =>
        (await _reposClient.SearchCommitsAsync(repositoryId, searchCriteria, top)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves the most recent commits from a specific branch in an Azure DevOps Git repository. Returns commit details including SHA, author, date, and message. Commonly used to get the latest commit SHA for branch operations or to check recent changes.")]
    public async Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1) =>
        (await _reposClient.GetLatestCommitsAsync(projectName, repositoryName, branchName, top)).EnsureSuccess(_logger);

    [McpServerTool, Description("Commits a new file to an Azure DevOps Git repository. Creates a new commit adding the specified file with its content to the target branch. Requires file path, content, commit message, and target branch. Returns the commit SHA of the new commit.")]
    public async Task<string> CommitAddFileAsync(FileCommitOptions options) =>
        (await _reposClient.CommitAddFileAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Creates a Git tag in an Azure DevOps repository to mark a specific commit, typically used for releases or important milestones. Tags are immutable references that don't move like branches. Requires tag name, target commit SHA, and optional message. Returns the created tag information.")]
    public async Task<GitAnnotatedTag> CreateTagAsync(TagCreateOptions options) =>
        (await _reposClient.CreateTagAsync(options)).EnsureSuccess(_logger);

    [McpServerTool, Description("Retrieves information about a specific Git tag from an Azure DevOps repository using its object ID. Returns tag details including name, target commit SHA, creator, and message. Useful for inspecting tag metadata or verifying tag existence.")]
    public async Task<GitAnnotatedTag> GetTagAsync(string repositoryId, string objectId) =>
        (await _reposClient.GetTagAsync(repositoryId, objectId)).EnsureSuccess(_logger);

    [McpServerTool, Description("Deletes a Git tag from an Azure DevOps repository. This permanently removes the tag reference but does not affect the underlying commit. Returns the result of the deletion operation. Use with caution as this action cannot be undone.")]
    public async Task<GitRefUpdateResult> DeleteTagAsync(string repositoryId, string tagName) =>
        (await _reposClient.DeleteTagAsync(repositoryId, tagName)).EnsureSuccess(_logger);

    [McpServerTool, Description("Adds one or more reviewers to an active pull request in Azure DevOps. Reviewers will be notified and can provide feedback, approve, or reject the pull request. Requires reviewer identities (local ID and display name). Returns true if all reviewers were added successfully.")]
    public async Task<bool> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers) =>
        (await _reposClient.AddReviewersAsync(repositoryId, pullRequestId, reviewers)).EnsureSuccess(_logger);

    [McpServerTool, Description("Removes specified reviewers from an active pull request in Azure DevOps. The reviewers will no longer receive notifications about the pull request and their previous votes/comments remain but they cannot vote further. Returns true if removal was successful.")]
    public async Task<bool> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds) =>
        (await _reposClient.RemoveReviewersAsync(repositoryId, pullRequestId, reviewerIds)).EnsureSuccess(_logger);

    [McpServerTool, Description("Sets or updates a reviewer's vote on a pull request in Azure DevOps. Vote values: 10=Approved, 5=Approved with suggestions, 0=No vote, -5=Waiting for author, -10=Rejected. The vote affects whether the pull request can be completed. Returns the updated reviewer information.")]
    public async Task<IdentityRefWithVote> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote) =>
        (await _reposClient.SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote)).EnsureSuccess(_logger);
}
