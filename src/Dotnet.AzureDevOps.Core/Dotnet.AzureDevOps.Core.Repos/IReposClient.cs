using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos;

public interface IReposClient
{
    Task<GitPullRequest> AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId);
    Task<IList<WebApiTagDefinition>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels);
    Task<AzureDevOpsActionResult<bool>> AddReviewerAsync(string repositoryId, int pullRequestId, (string localId, string name) reviewer);
    Task<AzureDevOpsActionResult<bool>> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers);
    Task<GitPullRequest> CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null);
    Task<List<GitRefUpdateResult>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha);
    Task<int> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions);
    Task<int?> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions);
    Task<Guid> CreateRepositoryAsync(string newRepositoryName);
    Task<GitAnnotatedTag> CreateTagAsync(TagCreateOptions tagCreateOptions);
    Task DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId);
    Task DeleteRepositoryAsync(Guid repositoryId);
    Task<GitRefUpdateResult?> DeleteTagAsync(string repositoryId, string tagName);
    Task<Comment> EditCommentAsync(CommentEditOptions commentEditOptions);
    Task<GitPullRequest> EnableAutoCompleteAsync(string repositoryId, int pullRequestId, string displayName, string localId, GitPullRequestCompletionOptions gitPullRequestCompletionOptions);
    Task<GitRef?> GetBranchAsync(string repositoryId, string branchName);
    Task<GitCommitDiffs> GetCommitDiffAsync(string repositoryId, string baseSha, string targetSha);
    Task<GitPullRequestIterationChanges> GetIterationChangesAsync(string repositoryId, int pullRequestId, int iteration);
    Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1);
    Task<GitPullRequest?> GetPullRequestAsync(string repositoryId, int pullRequestId);
    Task<IReadOnlyList<WebApiTagDefinition>> GetPullRequestLabelsAsync(string repository, int pullRequestId);
    Task<GitRepository?> GetRepositoryAsync(Guid repositoryId);
    Task<GitRepository?> GetRepositoryByNameAsync(string repositoryName);
    Task<GitAnnotatedTag> GetTagAsync(string repositoryId, string objectId);
    Task<IReadOnlyList<GitRef>> ListBranchesAsync(string repositoryId);
    Task<IReadOnlyList<GitPullRequestIteration>> ListIterationsAsync(string repositoryId, int pullRequestId);
    Task<IReadOnlyList<GitRef>> ListMyBranchesAsync(string repositoryId);
    Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions pullRequestSearchOptions);
    Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByLabelAsync(string repositoryId, string labelName, PullRequestStatus pullRequestStatus);
    Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByProjectAsync(PullRequestSearchOptions pullRequestSearchOptions);
    Task<IReadOnlyList<Comment>> ListPullRequestThreadCommentsAsync(string repositoryId, int pullRequestId, int threadId);
    Task<IReadOnlyList<GitPullRequestCommentThread>> ListPullRequestThreadsAsync(string repositoryId, int pullRequestId);
    Task<IReadOnlyList<GitRepository>> ListRepositoriesAsync();
    Task RemoveLabelAsync(string repositoryId, int pullRequestId, string label);
    Task RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds);
    Task<int?> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions);
    Task<GitPullRequestCommentThread> ResolveCommentThreadAsync(string repositoryId, int pullRequestId, int threadId);
    Task<IReadOnlyList<GitCommitRef>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100);
    Task<GitPullRequestStatus> SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions);
    Task<IdentityRefWithVote> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote);
    Task<GitPullRequest> UpdatePullRequestAsync(string repositoryId, int pullRequestId, PullRequestUpdateOptions pullRequestUpdateOptions);
    Task<string> CommitAddFileAsync(FileCommitOptions fileCommitOptions);
}
