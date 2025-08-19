using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public interface IReposClient : IDisposable, IAsyncDisposable
    {
        Task<AzureDevOpsActionResult<GitPullRequest>> AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, CancellationToken cancellationToken = default, params string[] labels);
        Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels);
        Task<AzureDevOpsActionResult<bool>> AddReviewerAsync(string repositoryId, int pullRequestId, (string localId, string name) reviewer, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<bool>> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<string>> CommitAddFileAsync(FileCommitOptions fileCommitOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitPullRequest>> CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<List<GitRefUpdateResult>>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<int>> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<int>> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<Guid>> CreateRepositoryAsync(string newRepositoryName, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitAnnotatedTag>> CreateTagAsync(TagCreateOptions tagCreateOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<bool>> DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<bool>> DeleteRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitRefUpdateResult>> DeleteTagAsync(string repositoryId, string tagName, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<Comment>> EditCommentAsync(CommentEditOptions commentEditOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitPullRequest>> EnableAutoCompleteAsync(string repositoryId, int pullRequestId, string displayName, string localId, GitPullRequestCompletionOptions gitPullRequestCompletionOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitRef>> GetBranchAsync(string repositoryId, string branchName, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitCommitDiffs>> GetCommitDiffAsync(string repositoryId, string baseSha, string targetSha, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitPullRequestIterationChanges>> GetIterationChangesAsync(string repositoryId, int pullRequestId, int iteration, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitPullRequest>> GetPullRequestAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>> GetPullRequestLabelsAsync(string repository, int pullRequestId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryByNameAsync(string repositoryName, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitAnnotatedTag>> GetTagAsync(string repositoryId, string objectId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListBranchesAsync(string repositoryId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>> ListIterationsAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListMyBranchesAsync(string repositoryId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions pullRequestSearchOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByLabelAsync(string repositoryId, string labelName, PullRequestStatus pullRequestStatus, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByProjectAsync(PullRequestSearchOptions pullRequestSearchOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<Comment>>> ListPullRequestThreadCommentsAsync(string repositoryId, int pullRequestId, int threadId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>> ListPullRequestThreadsAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitRepository>>> ListRepositoriesAsync(CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<bool>> RemoveLabelAsync(string repositoryId, int pullRequestId, string label, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds);
        Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken, params string[] reviewerIds);
        Task<AzureDevOpsActionResult<int>> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitPullRequestCommentThread>> ResolveCommentThreadAsync(string repositoryId, int pullRequestId, int threadId, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<GitPullRequestStatus>> SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions, CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote);
        Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote, CancellationToken cancellationToken);
        Task<AzureDevOpsActionResult<GitPullRequest>> UpdatePullRequestAsync(string repositoryId, int pullRequestId, PullRequestUpdateOptions pullRequestUpdateOptions, CancellationToken cancellationToken = default);
    }
}