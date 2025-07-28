using System.ComponentModel;
using System.Collections.Generic;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using ModelContextProtocol.Server;

namespace Dotnet.AzureDevOps.Mcp.Server.Tools;

/// <summary>
/// Exposes Git and pull request operations through Model Context Protocol.
/// </summary>
[McpServerToolType]
public class ReposTools
{
    private static ReposClient CreateClient(string organizationUrl, string projectName, string personalAccessToken)
        => new(organizationUrl, projectName, personalAccessToken);

    [McpServerTool, Description("Creates a pull request.")]
    public static Task<int?> CreatePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, PullRequestCreateOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreatePullRequestAsync(options);
    }

    [McpServerTool, Description("Retrieves a pull request.")]
    public static Task<GitPullRequest?> GetPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPullRequestAsync(repositoryId, pullRequestId);
    }

    [McpServerTool, Description("Updates a pull request.")]
    public static Task UpdatePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, PullRequestUpdateOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.UpdatePullRequestAsync(repositoryId, pullRequestId, options);
    }

    [McpServerTool, Description("Completes a pull request.")]
    public static Task CompletePullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, bool squashMerge = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CompletePullRequestAsync(repositoryId, pullRequestId, squashMerge, lastMergeSourceCommit, commitMessage);
    }

    [McpServerTool, Description("Abandons a pull request.")]
    public static Task AbandonPullRequestAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.AbandonPullRequestAsync(repositoryId, pullRequestId);
    }

    [McpServerTool, Description("Lists pull requests.")]
    public static Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, PullRequestSearchOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPullRequestsAsync(repositoryId, options);
    }

    [McpServerTool, Description("Adds reviewers to a pull request.")]
    public static Task AddReviewersAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, (string guid, string name)[] reviewers)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.AddReviewersAsync(repositoryId, pullRequestId, reviewers);
    }

    [McpServerTool, Description("Sets a reviewer vote.")]
    public static Task SetReviewerVoteAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string reviewerId, short vote)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote);
    }

    [McpServerTool, Description("Creates a comment thread on a pull request.")]
    public static Task<int> CreateCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, CommentThreadOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateCommentThreadAsync(options);
    }

    [McpServerTool, Description("Replies to a pull request comment thread.")]
    public static Task<int?> ReplyToCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, CommentReplyOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ReplyToCommentThreadAsync(options);
    }

    [McpServerTool, Description("Adds labels to a pull request.")]
    public static Task<IList<WebApiTagDefinition>> AddLabelsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, params string[] labels)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.AddLabelsAsync(repositoryId, pullRequestId, labels);
    }

    [McpServerTool, Description("Removes a label from a pull request.")]
    public static Task RemoveLabelAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, string label)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.RemoveLabelAsync(repositoryId, pullRequestId, label);
    }

    [McpServerTool, Description("Retrieves labels for a pull request.")]
    public static Task<IReadOnlyList<WebApiTagDefinition>> GetPullRequestLabelsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetPullRequestLabelsAsync(repositoryId, pullRequestId);
    }

    [McpServerTool, Description("Removes reviewers from a pull request.")]
    public static Task RemoveReviewersAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, params string[] reviewerIds)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.RemoveReviewersAsync(repositoryId, pullRequestId, reviewerIds);
    }

    [McpServerTool, Description("Sets pull request status.")]
    public static Task SetPullRequestStatusAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, PullRequestStatusOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.SetPullRequestStatusAsync(repositoryId, pullRequestId, options);
    }

    [McpServerTool, Description("Enables auto-complete on a pull request.")]
    public static Task EnableAutoCompleteAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, GitPullRequestCompletionOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.EnableAutoCompleteAsync(repositoryId, pullRequestId, options);
    }

    [McpServerTool, Description("Lists pull request iterations.")]
    public static Task<IReadOnlyList<GitPullRequestIteration>> ListIterationsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListIterationsAsync(repositoryId, pullRequestId);
    }

    [McpServerTool, Description("Gets changes for a pull request iteration.")]
    public static Task<GitPullRequestIterationChanges> GetIterationChangesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int iteration)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetIterationChangesAsync(repositoryId, pullRequestId, iteration);
    }

    [McpServerTool, Description("Lists pull requests filtered by label.")]
    public static Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByLabelAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string labelName, PullRequestStatus status)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPullRequestsByLabelAsync(repositoryId, labelName, status);
    }

    [McpServerTool, Description("Creates a branch in a repository.")]
    public static Task CreateBranchAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string newRefName, string baseCommitSha)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateBranchAsync(repositoryId, newRefName, baseCommitSha);
    }

    [McpServerTool, Description("Gets the diff between two commits.")]
    public static Task<GitCommitDiffs> GetCommitDiffAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string baseSha, string targetSha)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetCommitDiffAsync(repositoryId, baseSha, targetSha);
    }

    [McpServerTool, Description("Creates a new repository.")]
    public static Task<Guid> CreateRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, string newRepositoryName)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateRepositoryAsync(newRepositoryName);
    }

    [McpServerTool, Description("Deletes a repository.")]
    public static Task DeleteRepositoryAsync(string organizationUrl, string projectName, string personalAccessToken, Guid repositoryId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteRepositoryAsync(repositoryId);
    }

    [McpServerTool, Description("Edits a pull request comment.")]
    public static Task EditCommentAsync(string organizationUrl, string projectName, string personalAccessToken, CommentEditOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.EditCommentAsync(options);
    }

    [McpServerTool, Description("Deletes a pull request comment.")]
    public static Task DeleteCommentAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId, int commentId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteCommentAsync(repositoryId, pullRequestId, threadId, commentId);
    }

    [McpServerTool, Description("Creates an annotated tag in a repository.")]
    public static Task<GitAnnotatedTag> CreateTagAsync(string organizationUrl, string projectName, string personalAccessToken, TagCreateOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.CreateTagAsync(options);
    }

    [McpServerTool, Description("Gets an annotated tag by object id.")]
    public static Task<GitAnnotatedTag> GetTagAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string objectId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetTagAsync(repositoryId, objectId);
    }

    [McpServerTool, Description("Deletes a tag from a repository.")]
    public static Task<GitRefUpdateResult?> DeleteTagAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string tagName)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.DeleteTagAsync(repositoryId, tagName);
    }

    [McpServerTool, Description("Gets the latest commits from a branch.")]
    public static Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryName, string branchName, int top = 1)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetLatestCommitsAsync(projectName, repositoryName, branchName, top);
    }

    [McpServerTool, Description("Lists repositories in a project.")]
    public static Task<IReadOnlyList<GitRepository>> ListRepositoriesAsync(string organizationUrl, string projectName, string personalAccessToken)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListRepositoriesAsync();
    }

    [McpServerTool, Description("Lists pull requests across the project.")]
    public static Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByProjectAsync(string organizationUrl, string projectName, string personalAccessToken, PullRequestSearchOptions options)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPullRequestsByProjectAsync(options);
    }

    [McpServerTool, Description("Lists branches in a repository.")]
    public static Task<IReadOnlyList<GitRef>> ListBranchesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListBranchesAsync(repositoryId);
    }

    [McpServerTool, Description("Lists my branches in a repository.")]
    public static Task<IReadOnlyList<GitRef>> ListMyBranchesAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListMyBranchesAsync(repositoryId);
    }

    [McpServerTool, Description("Lists threads on a pull request.")]
    public static Task<IReadOnlyList<GitPullRequestCommentThread>> ListPullRequestThreadsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPullRequestThreadsAsync(repositoryId, pullRequestId);
    }

    [McpServerTool, Description("Lists comments in a pull request thread.")]
    public static Task<IReadOnlyList<Comment>> ListPullRequestThreadCommentsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPullRequestThreadCommentsAsync(repositoryId, pullRequestId, threadId);
    }

    [McpServerTool, Description("Gets a repository by name.")]
    public static Task<GitRepository?> GetRepositoryByNameAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryName)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetRepositoryByNameAsync(repositoryName);
    }

    [McpServerTool, Description("Gets a branch by name.")]
    public static Task<GitRef?> GetBranchAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, string branchName)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.GetBranchAsync(repositoryId, branchName);
    }

    [McpServerTool, Description("Resolves a comment thread.")]
    public static Task ResolveCommentThreadAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, int pullRequestId, int threadId)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ResolveCommentThreadAsync(repositoryId, pullRequestId, threadId);
    }

    [McpServerTool, Description("Searches commits in a repository.")]
    public static Task<IReadOnlyList<GitCommitRef>> SearchCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, GitQueryCommitsCriteria criteria, int top = 100)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.SearchCommitsAsync(repositoryId, criteria, top);
    }

    [McpServerTool, Description("Lists pull requests containing specific commits.")]
    public static Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByCommitsAsync(string organizationUrl, string projectName, string personalAccessToken, string repositoryId, IEnumerable<string> commitIds)
    {
        ReposClient client = CreateClient(organizationUrl, projectName, personalAccessToken);
        return client.ListPullRequestsByCommitsAsync(repositoryId, commitIds);
    }
}
