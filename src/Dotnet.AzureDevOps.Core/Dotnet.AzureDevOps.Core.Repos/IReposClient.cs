using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public interface IReposClient
    {
        Task<int?> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions);

        Task<GitPullRequest?> GetPullRequestAsync(string repositoryId, int pullRequestId);

        Task UpdatePullRequestAsync(string repositoryId, int pullRequestId, PullRequestUpdateOptions pullRequestUpdateOptions);

        Task CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null);

        Task AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId);

        Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions pullRequestSearchOptions);

        Task AddReviewersAsync(string repositoryId, int pullRequestId, (string guid, string name)[] reviewerInfos);

        Task SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote);

        Task<int> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions);

        Task<int?> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions);

        Task<IList<WebApiTagDefinition>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels);

        Task RemoveLabelAsync(string repositoryId, int pullRequestId, string label);

        Task RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds);

        Task SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions);

        Task EnableAutoCompleteAsync(string repositoryId, int pullRequestId, GitPullRequestCompletionOptions gitPullRequestCompletionOptions);

        Task<IReadOnlyList<GitPullRequestIteration>> ListIterationsAsync(string repositoryId, int pullRequestId);

        Task<GitPullRequestIterationChanges> GetIterationChangesAsync(string repositoryId, int pullRequestId, int iteration);

        Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByLabelAsync(string repositoryId, string labelName, PullRequestStatus pullRequestStatus);

        Task CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha);

        Task<GitCommitDiffs> GetCommitDiffAsync(string repositoryId, string baseSha, string targetSha);

        Task<Guid> CreateRepositoryAsync(string newRepoName);

        Task DeleteRepositoryAsync(Guid repositoryId);

        Task EditCommentAsync(CommentEditOptions commentEditOptions);

        Task DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId);

        Task<GitAnnotatedTag> GetTagAsync(string repositoryId, string objectId);

        Task<GitRefUpdateResult?> DeleteTagAsync(string repositoryId, string tagName);
    }
}
