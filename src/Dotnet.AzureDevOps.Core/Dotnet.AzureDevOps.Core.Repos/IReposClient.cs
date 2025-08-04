using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public interface IReposClient
    {
        Task<AzureDevOpsActionResult<GitPullRequest>> AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId);
        Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels);
        Task<AzureDevOpsActionResult<bool>> AddReviewerAsync(string repositoryId, int pullRequestId, (string localId, string name) reviewer);
        Task<AzureDevOpsActionResult<bool>> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers);
        Task<AzureDevOpsActionResult<string>> CommitAddFileAsync(FileCommitOptions fileCommitOptions);
        Task<AzureDevOpsActionResult<GitPullRequest>> CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null);
        Task<AzureDevOpsActionResult<List<GitRefUpdateResult>>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha);
        Task<AzureDevOpsActionResult<int>> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions);
        Task<AzureDevOpsActionResult<int>> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions);
        Task<AzureDevOpsActionResult<Guid>> CreateRepositoryAsync(string newRepositoryName);
        Task<AzureDevOpsActionResult<GitAnnotatedTag>> CreateTagAsync(TagCreateOptions tagCreateOptions);
        Task<AzureDevOpsActionResult<bool>> DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId);
        Task<AzureDevOpsActionResult<bool>> DeleteRepositoryAsync(Guid repositoryId);
        Task<AzureDevOpsActionResult<GitRefUpdateResult>> DeleteTagAsync(string repositoryId, string tagName);
        Task<AzureDevOpsActionResult<Comment>> EditCommentAsync(CommentEditOptions commentEditOptions);
        Task<AzureDevOpsActionResult<GitPullRequest>> EnableAutoCompleteAsync(string repositoryId, int pullRequestId, string displayName, string localId, GitPullRequestCompletionOptions gitPullRequestCompletionOptions);
        Task<AzureDevOpsActionResult<GitRef>> GetBranchAsync(string repositoryId, string branchName);
        Task<AzureDevOpsActionResult<GitCommitDiffs>> GetCommitDiffAsync(string repositoryId, string baseSha, string targetSha);
        Task<AzureDevOpsActionResult<GitPullRequestIterationChanges>> GetIterationChangesAsync(string repositoryId, int pullRequestId, int iteration);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1);
        Task<AzureDevOpsActionResult<GitPullRequest>> GetPullRequestAsync(string repositoryId, int pullRequestId);
        Task<AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>> GetPullRequestLabelsAsync(string repository, int pullRequestId);
        Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryAsync(Guid repositoryId);
        Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryByNameAsync(string repositoryName);
        Task<AzureDevOpsActionResult<GitAnnotatedTag>> GetTagAsync(string repositoryId, string objectId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListBranchesAsync(string repositoryId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>> ListIterationsAsync(string repositoryId, int pullRequestId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListMyBranchesAsync(string repositoryId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions pullRequestSearchOptions);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByLabelAsync(string repositoryId, string labelName, PullRequestStatus pullRequestStatus);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByProjectAsync(PullRequestSearchOptions pullRequestSearchOptions);
        Task<AzureDevOpsActionResult<IReadOnlyList<Comment>>> ListPullRequestThreadCommentsAsync(string repositoryId, int pullRequestId, int threadId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>> ListPullRequestThreadsAsync(string repositoryId, int pullRequestId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitRepository>>> ListRepositoriesAsync();
        Task<AzureDevOpsActionResult<bool>> RemoveLabelAsync(string repositoryId, int pullRequestId, string label);
        Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds);
        Task<AzureDevOpsActionResult<int>> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions);
        Task<AzureDevOpsActionResult<GitPullRequestCommentThread>> ResolveCommentThreadAsync(string repositoryId, int pullRequestId, int threadId);
        Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100);
        Task<AzureDevOpsActionResult<GitPullRequestStatus>> SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions);
        Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote);
        Task<AzureDevOpsActionResult<GitPullRequest>> UpdatePullRequestAsync(string repositoryId, int pullRequestId, PullRequestUpdateOptions pullRequestUpdateOptions);
    }
}