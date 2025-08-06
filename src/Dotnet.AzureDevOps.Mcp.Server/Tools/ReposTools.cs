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
    private static ReposClient CreateClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        => new(organizationUrl, projectName, personalAccessToken, logger);

    [McpServerTool, Description("Creates a pull request.")]
    public static async Task<int> CreatePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, PullRequestCreateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreatePullRequestAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a pull request.")]
    public static async Task<GitPullRequest> GetPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetPullRequestAsync(repositoryId, pullRequestId)).EnsureSuccess();
    }

    [McpServerTool, Description("Updates a pull request.")]
    public static async Task<GitPullRequest> UpdatePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, PullRequestUpdateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .UpdatePullRequestAsync(repositoryId, pullRequestId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Completes a pull request.")]
    public static async Task<GitPullRequest> CompletePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CompletePullRequestAsync(repositoryId, pullRequestId, squashMerge, deleteSourceBranch, lastMergeSourceCommit, commitMessage)).EnsureSuccess();
    }

    [McpServerTool, Description("Abandons a pull request.")]
    public static async Task<GitPullRequest> AbandonPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .AbandonPullRequestAsync(repositoryId, pullRequestId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists pull requests.")]
    public static async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, PullRequestSearchOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPullRequestsAsync(repositoryId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Adds reviewers to a pull request.")]
    public static async Task<bool> AddReviewersAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, (string guid, string name)[] reviewers, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .AddReviewersAsync(repositoryId, pullRequestId, reviewers)).EnsureSuccess();
    }

    [McpServerTool, Description("Sets a reviewer vote.")]
    public static async Task<IdentityRefWithVote> SetReviewerVoteAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string reviewerId, short vote, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a comment thread on a pull request.")]
    public static async Task<int> CreateCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, CommentThreadOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateCommentThreadAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Replies to a pull request comment thread.")]
    public static async Task<int> ReplyToCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, CommentReplyOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ReplyToCommentThreadAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Adds labels to a pull request.")]
    public static async Task<IList<WebApiTagDefinition>> AddLabelsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null, params string[] labels)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .AddLabelsAsync(repositoryId, pullRequestId, labels)).EnsureSuccess();
    }

    [McpServerTool, Description("Removes a label from a pull request.")]
    public static async Task<bool> RemoveLabelAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string label, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .RemoveLabelAsync(repositoryId, pullRequestId, label)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves labels for a pull request.")]
    public static async Task<IReadOnlyList<WebApiTagDefinition>> GetPullRequestLabelsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetPullRequestLabelsAsync(repositoryId, pullRequestId)).EnsureSuccess();
    }

    [McpServerTool, Description("Removes reviewers from a pull request.")]
    public static async Task<bool> RemoveReviewersAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null, params string[] reviewerIds)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .RemoveReviewersAsync(repositoryId, pullRequestId, reviewerIds)).EnsureSuccess();
    }

    [McpServerTool, Description("Sets pull request status.")]
    public static async Task<GitPullRequestStatus> SetPullRequestStatusAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, PullRequestStatusOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .SetPullRequestStatusAsync(repositoryId, pullRequestId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Enables auto-complete on a pull request.")]
    public static async Task<GitPullRequest> EnableAutoCompleteAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string displayName, string localId, GitPullRequestCompletionOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .EnableAutoCompleteAsync(repositoryId, pullRequestId, displayName, localId, options)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists pull request iterations.")]
    public static async Task<IReadOnlyList<GitPullRequestIteration>> ListIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListIterationsAsync(repositoryId, pullRequestId)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets changes for a pull request iteration.")]
    public static async Task<GitPullRequestIterationChanges> GetIterationChangesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int iteration, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetIterationChangesAsync(repositoryId, pullRequestId, iteration)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists pull requests filtered by label.")]
    public static async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByLabelAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string labelName, PullRequestStatus status, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPullRequestsByLabelAsync(repositoryId, labelName, status)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a branch in a repository.")]
    public static async Task<List<GitRefUpdateResult>> CreateBranchAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string newRefName, string baseCommitSha, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateBranchAsync(repositoryId, newRefName, baseCommitSha)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets the diff between two commits.")]
    public static async Task<GitCommitDiffs> GetCommitDiffAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string baseSha, string targetSha, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetCommitDiffAsync(repositoryId, baseSha, targetSha)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates a new repository.")]
    public static async Task<Guid> CreateRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, string newRepositoryName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateRepositoryAsync(newRepositoryName)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a repository.")]
    public static async Task<bool> DeleteRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, Guid repositoryId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteRepositoryAsync(repositoryId)).EnsureSuccess();
    }

    [McpServerTool, Description("Edits a pull request comment.")]
    public static async Task<Comment> EditCommentAsync(string organizationUrl, string projectName, string personalAccessToken, CommentEditOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .EditCommentAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a pull request comment.")]
    public static async Task<bool> DeleteCommentAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId, int commentId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteCommentAsync(repositoryId, pullRequestId, threadId, commentId)).EnsureSuccess();
    }

    [McpServerTool, Description("Creates an annotated tag in a repository.")]
    public static async Task<GitAnnotatedTag> CreateTagAsync(string organizationUrl, string projectName, string personalAccessToken, TagCreateOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CreateTagAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets an annotated tag by object id.")]
    public static async Task<GitAnnotatedTag> GetTagAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string objectId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetTagAsync(repositoryId, objectId)).EnsureSuccess();
    }

    [McpServerTool, Description("Deletes a tag from a repository.")]
    public static async Task<GitRefUpdateResult> DeleteTagAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string tagName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .DeleteTagAsync(repositoryId, tagName)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists repositories in a project.")]
    public static async Task<IReadOnlyList<GitRepository>> ListRepositoriesAsync(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListRepositoriesAsync()).EnsureSuccess();
    }

    [McpServerTool, Description("Lists pull requests across the project.")]
    public static async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByProjectAsync(string organizationUrl, string projectName, string personalAccessToken, PullRequestSearchOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPullRequestsByProjectAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists branches in a repository.")]
    public static async Task<IReadOnlyList<GitRef>> ListBranchesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListBranchesAsync(repositoryId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists my branches in a repository.")]
    public static async Task<IReadOnlyList<GitRef>> ListMyBranchesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListMyBranchesAsync(repositoryId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists threads on a pull request.")]
    public static async Task<IReadOnlyList<GitPullRequestCommentThread>> ListPullRequestThreadsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPullRequestThreadsAsync(repositoryId, pullRequestId)).EnsureSuccess();
    }

    [McpServerTool, Description("Lists comments in a pull request thread.")]
    public static async Task<IReadOnlyList<Comment>> ListPullRequestThreadCommentsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ListPullRequestThreadCommentsAsync(repositoryId, pullRequestId, threadId)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets a repository by name.")]
    public static async Task<GitRepository> GetRepositoryByNameAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetRepositoryByNameAsync(repositoryName)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets a branch by name.")]
    public static async Task<GitRef> GetBranchAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string branchName, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetBranchAsync(repositoryId, branchName)).EnsureSuccess();
    }

    [McpServerTool, Description("Resolves a comment thread.")]
    public static async Task<GitPullRequestCommentThread> ResolveCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .ResolveCommentThreadAsync(repositoryId, pullRequestId, threadId)).EnsureSuccess();
    }

    [McpServerTool, Description("Searches commits in a repository.")]
    public static async Task<IReadOnlyList<GitCommitRef>> SearchCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, GitQueryCommitsCriteria criteria, int top = 100, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .SearchCommitsAsync(repositoryId, criteria, top)).EnsureSuccess();
    }

    [McpServerTool, Description("Gets the latest commits from a branch.")]
    public static async Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryName, string branchName, int top = 1, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetLatestCommitsAsync(projectName, repositoryName, branchName, top)).EnsureSuccess();
    }

    [McpServerTool, Description("Adds a reviewer to a pull request.")]
    public static async Task<bool> AddReviewerAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, (string localId, string name) reviewer, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .AddReviewerAsync(repositoryId, pullRequestId, reviewer)).EnsureSuccess();
    }

    [McpServerTool, Description("Commits a new file to a repository.")]
    public static async Task<string> CommitAddFileAsync(string organizationUrl, string projectName, string personalAccessToken, FileCommitOptions options, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .CommitAddFileAsync(options)).EnsureSuccess();
    }

    [McpServerTool, Description("Retrieves a repository by identifier.")]
    public static async Task<GitRepository> GetRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, Guid repositoryId, ILogger? logger = null)
    {
        return (await CreateClient(organizationUrl, projectName, personalAccessToken, logger)
            .GetRepositoryAsync(repositoryId)).EnsureSuccess();
    }
}