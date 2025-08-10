using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<int>> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var newPullRequest = new GitPullRequest
                {
                    Title = pullRequestCreateOptions.Title,
                    Description = pullRequestCreateOptions.Description,
                    SourceRefName = pullRequestCreateOptions.SourceBranch,
                    TargetRefName = pullRequestCreateOptions.TargetBranch,
                    IsDraft = pullRequestCreateOptions.IsDraft
                };

                GitPullRequest createdPR = await _gitHttpClient.CreatePullRequestAsync(
                    gitPullRequestToCreate: newPullRequest,
                    repositoryId: pullRequestCreateOptions.RepositoryIdOrName,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<int>.Success(createdPR.PullRequestId, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                var pullRequestUpdate = new GitPullRequest
                {
                    Status = PullRequestStatus.Abandoned
                };

                GitPullRequest result = await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    repositoryId: repositoryIdOrName,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> GetPullRequestAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequest pr = await _gitHttpClient.GetPullRequestAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<GitPullRequest>.Success(pr, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> CompletePullRequestAsync(
            string repositoryId,
            int pullRequestId,
            bool squashMerge = false,
            bool deleteSourceBranch = false,
            GitCommitRef? lastMergeSourceCommit = null,
            string? commitMessage = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var pullRequestUpdate = new GitPullRequest
                {
                    LastMergeSourceCommit = lastMergeSourceCommit,
                    Status = PullRequestStatus.Completed,
                    CompletionOptions = new GitPullRequestCompletionOptions
                    {
                        SquashMerge = squashMerge,
                        DeleteSourceBranch = deleteSourceBranch,
                        MergeCommitMessage = commitMessage
                    }
                };

                GitPullRequest result = await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> UpdatePullRequestAsync(
            string repositoryId,
            int pullRequestId,
            PullRequestUpdateOptions pullRequestUpdateOptions,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var pullRequestUpdate = new GitPullRequest
                {
                    Title = pullRequestUpdateOptions.Title,
                    Description = pullRequestUpdateOptions.Description,
                    IsDraft = pullRequestUpdateOptions.IsDraft
                };

                if(pullRequestUpdateOptions.ReviewerIds is { } reviewers)
                    pullRequestUpdate.Reviewers = [.. reviewers.Select(id => new IdentityRefWithVote { Id = id })];

                GitPullRequest result = await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsAsync(
            string repositoryId, PullRequestSearchOptions pullRequestSearchOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var criteria = new GitPullRequestSearchCriteria
                {
                    Status = pullRequestSearchOptions.Status,
                    TargetRefName = pullRequestSearchOptions.TargetBranch,
                    SourceRefName = pullRequestSearchOptions.SourceBranch
                };

                List<GitPullRequest> pullRequests = await _gitHttpClient.GetPullRequestsAsync(
                    repositoryId: repositoryId,
                    searchCriteria: criteria,
                    top: 1000,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(pullRequests, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByProjectAsync(PullRequestSearchOptions pullRequestSearchOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var searchCriteria = new GitPullRequestSearchCriteria
                {
                    Status = pullRequestSearchOptions.Status,
                    TargetRefName = pullRequestSearchOptions.TargetBranch,
                    SourceRefName = pullRequestSearchOptions.SourceBranch
                };

                List<GitPullRequest> pullRequests = await _gitHttpClient.GetPullRequestsByProjectAsync(
                    project: ProjectName,
                    searchCriteria: searchCriteria,
                    top: 1000,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(pullRequests, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByLabelAsync(
            string repositoryId, string labelName, PullRequestStatus pullRequestStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> allPullRequestsResult = await ListPullRequestsAsync(
                    repositoryId, new PullRequestSearchOptions { Status = pullRequestStatus }, cancellationToken);

                if(!allPullRequestsResult.IsSuccessful || allPullRequestsResult.Value == null)
                    return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(allPullRequestsResult.ErrorMessage ?? "Failed to list pull requests.", Logger);

                List<GitPullRequest> filtered = allPullRequestsResult.Value.Where(pullRequest => pullRequest.Labels.Any(label =>
                    string.Equals(label.Name, labelName, StringComparison.OrdinalIgnoreCase))).ToList();

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(filtered, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestStatus>> SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var status = new GitPullRequestStatus
                {
                    Context = new GitStatusContext { Name = pullRequestStatusOptions.ContextName, Genre = pullRequestStatusOptions.ContextGenre },
                    State = pullRequestStatusOptions.State,
                    Description = pullRequestStatusOptions.Description,
                    TargetUrl = pullRequestStatusOptions.TargetUrl
                };

                GitPullRequestStatus result = await _gitHttpClient.CreatePullRequestStatusAsync(
                    status: status,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<GitPullRequestStatus>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestStatus>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> EnableAutoCompleteAsync(
            string repositoryId,
            int pullRequestId,
            string displayName,
            string localId,
            GitPullRequestCompletionOptions gitPullRequestCompletionOptions,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var pullRequestUpdate = new GitPullRequest
                {
                    AutoCompleteSetBy = new IdentityRef { DisplayName = displayName, Id = localId },
                    CompletionOptions = gitPullRequestCompletionOptions
                };

                GitPullRequest result = await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>> ListIterationsAsync(
            string repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitPullRequestIteration> result = await _gitHttpClient.GetPullRequestIterationsAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestIterationChanges>> GetIterationChangesAsync(
            string repositoryId, int pullRequestId, int iteration, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequestIterationChanges result = await _gitHttpClient.GetPullRequestIterationChangesAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    iterationId: iteration,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<GitPullRequestIterationChanges>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestIterationChanges>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> AddReviewerAsync(string repositoryId, int pullRequestId, (string localId, string name) reviewer, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrEmpty(reviewer.localId))
                return AzureDevOpsActionResult<bool>.Failure("Reviewer localId must be provided", Logger);

            var reviewerPayload = new
            {
                localId = reviewer.localId,
                DisplayName = reviewer.name ?? string.Empty
            };

            string requestUrl = $"{OrganizationUrl}/{ProjectName}/_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/reviewers/{reviewer.localId}?api-version={GlobalConstants.ApiVersion}";

            string content = JsonSerializer.Serialize(reviewerPayload);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = await _httpClient.PutAsync(requestUrl, httpContent, cancellationToken);
                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, Logger);
                }
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers, CancellationToken cancellationToken = default)
        {
            if(reviewers is null || reviewers.Length == 0)
                return AzureDevOpsActionResult<bool>.Failure("No reviewers specified", Logger);

            bool allAdded = true;
            foreach((string localId, string name) reviewer in reviewers)
            {
                AzureDevOpsActionResult<bool> result = await AddReviewerAsync(repositoryId, pullRequestId, reviewer, cancellationToken);
                if(!result.IsSuccessful || !result.Value)
                    allAdded = false;
            }

            return allAdded
                ? AzureDevOpsActionResult<bool>.Success(true, Logger)
                : AzureDevOpsActionResult<bool>.Failure("One or more reviewers could not be added", Logger);
        }

        public async Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote)
        {
            return await SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote, CancellationToken.None);
        }

        public async Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote, CancellationToken cancellationToken)
        {
            try
            {
                var reviewerUpdate = new IdentityRefWithVote
                {
                    Id = reviewerId,
                    Vote = vote
                };

                IdentityRefWithVote result = await _gitHttpClient.CreatePullRequestReviewerAsync(
                    reviewer: reviewerUpdate,
                    repositoryId: repositoryId,
                    reviewerId: reviewerId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IdentityRefWithVote>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IdentityRefWithVote>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds)
        {
            return await RemoveReviewersAsync(repositoryId, pullRequestId, CancellationToken.None, reviewerIds);
        }

        public async Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken, params string[] reviewerIds)
        {
            try
            {
                foreach(string id in reviewerIds)
                    await _gitHttpClient.DeletePullRequestReviewerAsync(
                        repositoryId: repositoryId,
                        reviewerId: id,
                        pullRequestId: pullRequestId,
                        project: ProjectName);
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<int>> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var comment = new Comment
                {
                    Content = commentThreadOptions.Comment,
                    CommentType = CommentType.Text
                };

                var commentThread = new GitPullRequestCommentThread
                {
                    Comments = [comment],
                    Status = CommentThreadStatus.Active
                };

                if(!string.IsNullOrWhiteSpace(commentThreadOptions.FilePath))
                {
                    commentThread.ThreadContext = new CommentThreadContext
                    {
                        FilePath = commentThreadOptions.FilePath,
                        RightFileStart = new CommentPosition { Line = 1, Offset = 1 },
                        RightFileEnd = new CommentPosition { Line = 1, Offset = 1 }
                    };

                    if(commentThreadOptions.IsLeftSide)
                        commentThread.ThreadContext.LeftFileStart = commentThread.ThreadContext.LeftFileEnd = new CommentPosition { Line = 1, Offset = 1 };
                }

                GitPullRequestCommentThread created = await _gitHttpClient.CreateThreadAsync(
                    commentThread: commentThread,
                    repositoryId: commentThreadOptions.RepositoryId,
                    pullRequestId: commentThreadOptions.PullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<int>.Success(created.Id, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<int>> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var newComment = new Comment
                {
                    Content = commentReplyOptions.Comment,
                    CommentType = CommentType.Text
                };

                GitPullRequestCommentThread gitPullRequestCommentThread;

                if(commentReplyOptions.ResolveThread)
                {
                    gitPullRequestCommentThread = new GitPullRequestCommentThread
                    {
                        Id = commentReplyOptions.ThreadId,
                        Comments = [newComment],
                        Status = CommentThreadStatus.Fixed
                    };

                    gitPullRequestCommentThread = await _gitHttpClient.UpdateThreadAsync(
                        gitPullRequestCommentThread,
                        repositoryId: commentReplyOptions.Repository,
                        pullRequestId: commentReplyOptions.PullRequestId,
                        threadId: commentReplyOptions.ThreadId,
                        project: ProjectName,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await _gitHttpClient.CreateCommentAsync(
                        comment: newComment,
                        repositoryId: commentReplyOptions.Repository,
                        pullRequestId: commentReplyOptions.PullRequestId,
                        threadId: commentReplyOptions.ThreadId,
                        project: ProjectName,
                        cancellationToken: cancellationToken);

                    gitPullRequestCommentThread = await _gitHttpClient.GetPullRequestThreadAsync(
                        repositoryId: commentReplyOptions.Repository,
                        pullRequestId: commentReplyOptions.PullRequestId,
                        threadId: commentReplyOptions.ThreadId,
                        project: ProjectName,
                        cancellationToken: cancellationToken);
                }

                return AzureDevOpsActionResult<int>.Success(gitPullRequestCommentThread.Id, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<Comment>> EditCommentAsync(CommentEditOptions commentEditOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                var updatedComment = new Comment
                {
                    Content = commentEditOptions.NewContent
                };

                Comment result = await _gitHttpClient.UpdateCommentAsync(
                    comment: updatedComment,
                    repositoryId: commentEditOptions.Repository,
                    pullRequestId: commentEditOptions.PullRequest,
                    threadId: commentEditOptions.ThreadId,
                    commentId: commentEditOptions.CommentId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<Comment>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Comment>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _gitHttpClient.DeleteCommentAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId,
                    commentId: commentId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>> ListPullRequestThreadsAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitPullRequestCommentThread> result = await _gitHttpClient.GetThreadsAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<Comment>>> ListPullRequestThreadCommentsAsync(string repositoryId, int pullRequestId, int threadId, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<Comment> result = await _gitHttpClient.GetCommentsAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<Comment>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<Comment>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestCommentThread>> ResolveCommentThreadAsync(string repositoryId, int pullRequestId, int threadId, CancellationToken cancellationToken = default)
        {
            try
            {
                var threadUpdate = new GitPullRequestCommentThread
                {
                    Status = CommentThreadStatus.Fixed
                };

                GitPullRequestCommentThread result = await _gitHttpClient.UpdateThreadAsync(
                    commentThread: threadUpdate,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<GitPullRequestCommentThread>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestCommentThread>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels)
        {
            return await AddLabelsAsync(repository, pullRequestId, CancellationToken.None, labels);
        }

        public async Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, CancellationToken cancellationToken = default, params string[] labels)
        {
            try
            {
                var webApiTagDefinitions = new List<WebApiTagDefinition>();
                foreach(string label in labels.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var tagRequestData = new WebApiCreateTagRequestData { Name = label };

                    WebApiTagDefinition webApiTagDefinition = await _gitHttpClient.CreatePullRequestLabelAsync(
                        label: tagRequestData,
                        project: ProjectName,
                        repositoryId: repository,
                        pullRequestId: pullRequestId,
                        cancellationToken: cancellationToken
                    );

                    webApiTagDefinitions.Add(webApiTagDefinition);
                }

                return AzureDevOpsActionResult<IList<WebApiTagDefinition>>.Success(webApiTagDefinitions, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IList<WebApiTagDefinition>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> RemoveLabelAsync(string repositoryId, int pullRequestId, string label, CancellationToken cancellationToken = default)
        {
            try
            {
                await _gitHttpClient.DeletePullRequestLabelsAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    labelIdOrName: label,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>> GetPullRequestLabelsAsync(string repository, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<WebApiTagDefinition> result = await _gitHttpClient.GetPullRequestLabelsAsync(
                    project: ProjectName,
                    repositoryId: repository,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>.Failure(ex, Logger);
            }
        }
    }
}


