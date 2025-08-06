using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

[McpServerToolType]
public class ReposTools
{
    private readonly IReposClient _reposClient;
    private readonly ILogger<ReposTools> _logger;

    public ReposTools(IReposClient reposClient, ILogger<ReposTools> logger)
    {
        _reposClient = reposClient;
        _logger = logger;
    }

    [McpServerTool, Description("Creates a pull request.")]
    public async Task<int> CreatePullRequestAsync(PullRequestCreateOptions options)
    {
        return (await _reposClient.CreatePullRequestAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a pull request.")]
    public async Task<GitPullRequest> GetPullRequestAsync(string repositoryId, int pullRequestId)
    {
        return (await _reposClient.GetPullRequestAsync(repositoryId, pullRequestId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Completes a pull request.")]
    public async Task<GitPullRequest> CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null)
    {
        return (await _reposClient.CompletePullRequestAsync(repositoryId, pullRequestId, squashMerge, deleteSourceBranch, lastMergeSourceCommit, commitMessage)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Abandons a pull request.")]
    public async Task<GitPullRequest> AbandonPullRequestAsync(string repositoryId, int pullRequestId)
    {
        return (await _reposClient.AbandonPullRequestAsync(repositoryId, pullRequestId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists pull requests.")]
    public async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions options)
    {
        return (await _reposClient.ListPullRequestsAsync(repositoryId, options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a new repository.")]
    public async Task<Guid> CreateRepositoryAsync(string repositoryName)
    {
        return (await _reposClient.CreateRepositoryAsync(repositoryName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a repository by ID.")]
    public async Task<GitRepository> GetRepositoryAsync(Guid repositoryId)
    {
        return (await _reposClient.GetRepositoryAsync(repositoryId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves a repository by name.")]
    public async Task<GitRepository> GetRepositoryByNameAsync(string repositoryName)
    {
        return (await _reposClient.GetRepositoryByNameAsync(repositoryName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves all repositories in the project.")]
    public async Task<IReadOnlyList<GitRepository>> ListRepositoriesAsync()
    {
        return (await _reposClient.ListRepositoriesAsync()).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a repository.")]
    public async Task<bool> DeleteRepositoryAsync(Guid repositoryId)
    {
        return (await _reposClient.DeleteRepositoryAsync(repositoryId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Retrieves branches from a repository.")]
    public async Task<IReadOnlyList<GitRef>> ListBranchesAsync(string repositoryId)
    {
        return (await _reposClient.ListBranchesAsync(repositoryId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a new branch.")]
    public async Task<List<GitRefUpdateResult>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha)
    {
        return (await _reposClient.CreateBranchAsync(repositoryId, newRefName, baseCommitSha)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets a specific branch.")]
    public async Task<GitRef> GetBranchAsync(string repositoryId, string branchName)
    {
        return (await _reposClient.GetBranchAsync(repositoryId, branchName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Lists commits matching search criteria.")]
    public async Task<IReadOnlyList<GitCommitRef>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100)
    {
        return (await _reposClient.SearchCommitsAsync(repositoryId, searchCriteria, top)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets latest commits for a branch.")]
    public async Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1)
    {
        return (await _reposClient.GetLatestCommitsAsync(projectName, repositoryName, branchName, top)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Commits a file addition.")]
    public async Task<string> CommitAddFileAsync(FileCommitOptions options)
    {
        return (await _reposClient.CommitAddFileAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Creates a tag.")]
    public async Task<GitAnnotatedTag> CreateTagAsync(TagCreateOptions options)
    {
        return (await _reposClient.CreateTagAsync(options)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Gets a tag.")]
    public async Task<GitAnnotatedTag> GetTagAsync(string repositoryId, string objectId)
    {
        return (await _reposClient.GetTagAsync(repositoryId, objectId)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Deletes a tag.")]
    public async Task<GitRefUpdateResult> DeleteTagAsync(string repositoryId, string tagName)
    {
        return (await _reposClient.DeleteTagAsync(repositoryId, tagName)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Adds reviewers to a pull request.")]
    public async Task<bool> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers)
    {
        return (await _reposClient.AddReviewersAsync(repositoryId, pullRequestId, reviewers)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Removes reviewers from a pull request.")]
    public async Task<bool> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds)
    {
        return (await _reposClient.RemoveReviewersAsync(repositoryId, pullRequestId, reviewerIds)).EnsureSuccess(_logger);
    }

    [McpServerTool, Description("Sets a reviewer's vote on a pull request.")]
    public async Task<IdentityRefWithVote> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote)
    {
        return (await _reposClient.SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote)).EnsureSuccess(_logger);
    }
}
