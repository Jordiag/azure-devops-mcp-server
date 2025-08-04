using System.ComponentModel;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

[McpServerToolType]
public class ReposTools
{
    private static ReposClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a pull request.")]
    public static async Task<int?> CreatePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, PullRequestCreateOptions options)
    {
        AzureDevOpsActionResult<int> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreatePullRequestAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create pull request.");
        return result.Value;
    }

    [McpServerTool, Description("Retrieves a pull request.")]
    public static async Task<GitPullRequest?> GetPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        AzureDevOpsActionResult<GitPullRequest> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetPullRequestAsync(repositoryId, pullRequestId);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Updates a pull request.")]
    public static async Task UpdatePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, PullRequestUpdateOptions options)
    {
        AzureDevOpsActionResult<GitPullRequest> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .UpdatePullRequestAsync(repositoryId, pullRequestId, options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to update pull request.");
    }

    [McpServerTool, Description("Completes a pull request.")]
    public static async Task CompletePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null)
    {
        AzureDevOpsActionResult<GitPullRequest> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CompletePullRequestAsync(repositoryId, pullRequestId, squashMerge, deleteSourceBranch, lastMergeSourceCommit, commitMessage);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to complete pull request.");
    }

    [McpServerTool, Description("Abandons a pull request.")]
    public static async Task AbandonPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        AzureDevOpsActionResult<GitPullRequest> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .AbandonPullRequestAsync(repositoryId, pullRequestId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to abandon pull request.");
    }

    [McpServerTool, Description("Lists pull requests.")]
    public static async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, PullRequestSearchOptions options)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListPullRequestsAsync(repositoryId, options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list pull requests.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Adds reviewers to a pull request.")]
    public static async Task AddReviewersAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, (string guid, string name)[] reviewers)
    {
        AzureDevOpsActionResult<bool> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .AddReviewersAsync(repositoryId, pullRequestId, reviewers);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to add reviewers.");
    }

    [McpServerTool, Description("Sets a reviewer vote.")]
    public static async Task SetReviewerVoteAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string reviewerId, short vote)
    {
        AzureDevOpsActionResult<IdentityRefWithVote> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to set reviewer vote.");
    }

    [McpServerTool, Description("Creates a comment thread on a pull request.")]
    public static async Task<int> CreateCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, CommentThreadOptions options)
    {
        AzureDevOpsActionResult<int> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateCommentThreadAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create comment thread.");
        return result.Value;
    }

    [McpServerTool, Description("Replies to a pull request comment thread.")]
    public static async Task<int?> ReplyToCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, CommentReplyOptions options)
    {
        AzureDevOpsActionResult<int> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ReplyToCommentThreadAsync(options);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Adds labels to a pull request.")]
    public static async Task<IList<WebApiTagDefinition>> AddLabelsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, params string[] labels)
    {
        AzureDevOpsActionResult<IList<WebApiTagDefinition>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .AddLabelsAsync(repositoryId, pullRequestId, labels);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to add labels.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Removes a label from a pull request.")]
    public static async Task RemoveLabelAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string label)
    {
        AzureDevOpsActionResult<bool> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .RemoveLabelAsync(repositoryId, pullRequestId, label);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to remove label.");
    }

    [McpServerTool, Description("Retrieves labels for a pull request.")]
    public static async Task<IReadOnlyList<WebApiTagDefinition>> GetPullRequestLabelsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetPullRequestLabelsAsync(repositoryId, pullRequestId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get pull request labels.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Removes reviewers from a pull request.")]
    public static async Task RemoveReviewersAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, params string[] reviewerIds)
    {
        AzureDevOpsActionResult<bool> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .RemoveReviewersAsync(repositoryId, pullRequestId, reviewerIds);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to remove reviewers.");
    }

    [McpServerTool, Description("Sets pull request status.")]
    public static async Task SetPullRequestStatusAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, PullRequestStatusOptions options)
    {
        AzureDevOpsActionResult<GitPullRequestStatus> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .SetPullRequestStatusAsync(repositoryId, pullRequestId, options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to set pull request status.");
    }

    [McpServerTool, Description("Enables auto-complete on a pull request.")]
    public static async Task EnableAutoCompleteAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string displayName, string localId, GitPullRequestCompletionOptions options)
    {
        AzureDevOpsActionResult<GitPullRequest> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .EnableAutoCompleteAsync(repositoryId, pullRequestId, displayName, localId, options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to enable auto-complete.");
    }

    [McpServerTool, Description("Lists pull request iterations.")]
    public static async Task<IReadOnlyList<GitPullRequestIteration>> ListIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListIterationsAsync(repositoryId, pullRequestId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list iterations.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Gets changes for a pull request iteration.")]
    public static async Task<GitPullRequestIterationChanges> GetIterationChangesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int iteration)
    {
        AzureDevOpsActionResult<GitPullRequestIterationChanges> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetIterationChangesAsync(repositoryId, pullRequestId, iteration);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get iteration changes.");
        return result.Value;
    }

    [McpServerTool, Description("Lists pull requests filtered by label.")]
    public static async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByLabelAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string labelName, PullRequestStatus status)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListPullRequestsByLabelAsync(repositoryId, labelName, status);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list pull requests by label.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Creates a branch in a repository.")]
    public static async Task CreateBranchAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string newRefName, string baseCommitSha)
    {
        AzureDevOpsActionResult<List<GitRefUpdateResult>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateBranchAsync(repositoryId, newRefName, baseCommitSha);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create branch.");
    }

    [McpServerTool, Description("Gets the diff between two commits.")]
    public static async Task<GitCommitDiffs> GetCommitDiffAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string baseSha, string targetSha)
    {
        AzureDevOpsActionResult<GitCommitDiffs> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetCommitDiffAsync(repositoryId, baseSha, targetSha);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get commit diff.");
        return result.Value;
    }

    [McpServerTool, Description("Creates a new repository.")]
    public static async Task<Guid> CreateRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, string newRepositoryName)
    {
        AzureDevOpsActionResult<Guid> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateRepositoryAsync(newRepositoryName);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create repository.");
        return result.Value;
    }

    [McpServerTool, Description("Deletes a repository.")]
    public static async Task DeleteRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, Guid repositoryId)
    {
        AzureDevOpsActionResult<bool> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeleteRepositoryAsync(repositoryId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete repository.");
    }

    [McpServerTool, Description("Edits a pull request comment.")]
    public static async Task EditCommentAsync(string organizationUrl, string projectName, string personalAccessToken, CommentEditOptions options)
    {
        AzureDevOpsActionResult<Comment> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .EditCommentAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to edit comment.");
    }

    [McpServerTool, Description("Deletes a pull request comment.")]
    public static async Task DeleteCommentAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId, int commentId)
    {
        AzureDevOpsActionResult<bool> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeleteCommentAsync(repositoryId, pullRequestId, threadId, commentId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete comment.");
    }

    [McpServerTool, Description("Creates an annotated tag in a repository.")]
    public static async Task<GitAnnotatedTag> CreateTagAsync(string organizationUrl, string projectName, string personalAccessToken, TagCreateOptions options)
    {
        AzureDevOpsActionResult<GitAnnotatedTag> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .CreateTagAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create tag.");
        return result.Value;
    }

    [McpServerTool, Description("Gets an annotated tag by object id.")]
    public static async Task<GitAnnotatedTag> GetTagAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string objectId)
    {
        AzureDevOpsActionResult<GitAnnotatedTag> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetTagAsync(repositoryId, objectId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get tag.");
        return result.Value;
    }

    [McpServerTool, Description("Deletes a tag from a repository.")]
    public static async Task<GitRefUpdateResult?> DeleteTagAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string tagName)
    {
        AzureDevOpsActionResult<GitRefUpdateResult> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .DeleteTagAsync(repositoryId, tagName);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to delete tag.");
        return result.Value;
    }

    [McpServerTool, Description("Lists repositories in a project.")]
    public static async Task<IReadOnlyList<GitRepository>> ListRepositoriesAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitRepository>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListRepositoriesAsync();
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list repositories.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Lists pull requests across the project.")]
    public static async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByProjectAsync(string organizationUrl, string projectName, string personalAccessToken, PullRequestSearchOptions options)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListPullRequestsByProjectAsync(options);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list pull requests by project.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Lists branches in a repository.")]
    public static async Task<IReadOnlyList<GitRef>> ListBranchesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitRef>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListBranchesAsync(repositoryId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list branches.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Lists my branches in a repository.")]
    public static async Task<IReadOnlyList<GitRef>> ListMyBranchesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitRef>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListMyBranchesAsync(repositoryId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list my branches.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Lists threads on a pull request.")]
    public static async Task<IReadOnlyList<GitPullRequestCommentThread>> ListPullRequestThreadsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListPullRequestThreadsAsync(repositoryId, pullRequestId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list pull request threads.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Lists comments in a pull request thread.")]
    public static async Task<IReadOnlyList<Comment>> ListPullRequestThreadCommentsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId)
    {
        AzureDevOpsActionResult<IReadOnlyList<Comment>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ListPullRequestThreadCommentsAsync(repositoryId, pullRequestId, threadId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to list pull request thread comments.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Gets a repository by name.")]
    public static async Task<GitRepository?> GetRepositoryByNameAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryName)
    {
        AzureDevOpsActionResult<GitRepository> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetRepositoryByNameAsync(repositoryName);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Gets a branch by name.")]
    public static async Task<GitRef?> GetBranchAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string branchName)
    {
        AzureDevOpsActionResult<GitRef> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetBranchAsync(repositoryId, branchName);
        if(!result.IsSuccessful)
            return null;
        return result.Value;
    }

    [McpServerTool, Description("Resolves a comment thread.")]
    public static async Task ResolveCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId)
    {
        AzureDevOpsActionResult<GitPullRequestCommentThread> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .ResolveCommentThreadAsync(repositoryId, pullRequestId, threadId);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to resolve comment thread.");
    }

    [McpServerTool, Description("Searches commits in a repository.")]
    public static async Task<IReadOnlyList<GitCommitRef>> SearchCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, GitQueryCommitsCriteria criteria, int top = 100)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .SearchCommitsAsync(repositoryId, criteria, top);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to search commits.");
        return result.Value ?? [];
    }

    [McpServerTool, Description("Gets the latest commits from a branch.")]
    public static async Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryName, string branchName, int top = 1)
    {
        AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> result = await CreateClient(organizationUrl, projectName, personalAccessToken)
            .GetLatestCommitsAsync(projectName, repositoryName, branchName, top);
        if(!result.IsSuccessful)
            throw new InvalidOperationException(result.ErrorMessage ?? "Failed to get latest commits.");
        return result.Value ?? [];
    }
}