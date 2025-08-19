using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
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
                int id = await ExecuteWithExceptionHandlingAsync(async () =>
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
                        cancellationToken: cancellationToken);

                    return createdPR.PullRequestId;
                }, "CreatePullRequest", OperationType.Create);

                return AzureDevOpsActionResult<int>.Success(id, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> GetPullRequestAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequest pr = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.GetPullRequestAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken
                );
                }, "GetPullRequest", OperationType.Read);

                return AzureDevOpsActionResult<GitPullRequest>.Success(pr, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId, CancellationToken cancellationToken = default)
        {
            var pullRequestUpdate = new GitPullRequest { Status = PullRequestStatus.Abandoned };
            try
            {
                GitPullRequest result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.UpdatePullRequestAsync(
                            gitPullRequestToUpdate: pullRequestUpdate,
                            repositoryId: repositoryIdOrName,
                            pullRequestId: pullRequestId,
                            project: ProjectName,
                            cancellationToken: cancellationToken);
                }, "AbandonPullRequest", OperationType.Read);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> CompletePullRequestAsync(string repositoryId, int pullRequestId, bool squashMerge = false, bool deleteSourceBranch = false, GitCommitRef? lastMergeSourceCommit = null, string? commitMessage = null, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequest result = await ExecuteWithExceptionHandlingAsync(async () =>
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
                    return await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken
                );
                }, "CompletePullRequest", OperationType.Update);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> UpdatePullRequestAsync(string repositoryId, int pullRequestId, PullRequestUpdateOptions pullRequestUpdateOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequest result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var pullRequestUpdate = new GitPullRequest
                    {
                        Title = pullRequestUpdateOptions.Title,
                        Description = pullRequestUpdateOptions.Description,
                        IsDraft = pullRequestUpdateOptions.IsDraft
                    };
                    if(pullRequestUpdateOptions.ReviewerIds is { } reviewers)
                    {
                        pullRequestUpdate.Reviewers = reviewers.Select(id => new IdentityRefWithVote { Id = id }).ToArray();
                    }
                    return await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "UpdatePullRequest", OperationType.Update);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsAsync(string repositoryId, PullRequestSearchOptions pullRequestSearchOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitPullRequest> result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var criteria = new GitPullRequestSearchCriteria
                    {
                        Status = pullRequestSearchOptions.Status,
                        TargetRefName = pullRequestSearchOptions.TargetBranch,
                        SourceRefName = pullRequestSearchOptions.SourceBranch
                    };
                    List<GitPullRequest> list = await _gitHttpClient.GetPullRequestsAsync(
                    repositoryId: repositoryId,
                    searchCriteria: criteria,
                    top: 1000,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                    return (IReadOnlyList<GitPullRequest>)list;
                }, "ListPullRequests", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(result, Logger);
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
                IReadOnlyList<GitPullRequest> result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    GitPullRequestSearchCriteria searchCriteria = new GitPullRequestSearchCriteria
                    {
                        Status = pullRequestSearchOptions.Status,
                        TargetRefName = pullRequestSearchOptions.TargetBranch,
                        SourceRefName = pullRequestSearchOptions.SourceBranch
                    };
                    List<GitPullRequest> list = await _gitHttpClient.GetPullRequestsByProjectAsync(
                    project: ProjectName,
                    searchCriteria: searchCriteria,
                    top: 1000,
                    cancellationToken: cancellationToken);
                    return (IReadOnlyList<GitPullRequest>)list;
                }, "ListPullRequestsByProject", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByLabelAsync(string repositoryId, string labelName, PullRequestStatus pullRequestStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> all = await ListPullRequestsAsync(repositoryId, new PullRequestSearchOptions { Status = pullRequestStatus }, cancellationToken);
                if(!all.IsSuccessful || all.Value == null)
                    return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(all.ErrorMessage ?? "Failed to list pull requests.", Logger);

                IReadOnlyList<GitPullRequest> filtered = all.Value
                    .Where(pr => pr.Labels.Any(l => string.Equals(l.Name, labelName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

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
                GitPullRequestStatus statusResult = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var status = new GitPullRequestStatus
                    {
                        Context = new GitStatusContext { Name = pullRequestStatusOptions.ContextName, Genre = pullRequestStatusOptions.ContextGenre },
                        State = pullRequestStatusOptions.State,
                        Description = pullRequestStatusOptions.Description,
                        TargetUrl = pullRequestStatusOptions.TargetUrl
                    };
                    return await _gitHttpClient.CreatePullRequestStatusAsync(status, repositoryId, pullRequestId, ProjectName, cancellationToken);
                }, "SetPullRequestStatus", OperationType.Update);

                return AzureDevOpsActionResult<GitPullRequestStatus>.Success(statusResult, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestStatus>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> EnableAutoCompleteAsync(string repositoryId, int pullRequestId, string displayName, string localId, GitPullRequestCompletionOptions gitPullRequestCompletionOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequest result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var pullRequestUpdate = new GitPullRequest
                    {
                        AutoCompleteSetBy = new IdentityRef { DisplayName = displayName, Id = localId },
                        CompletionOptions = gitPullRequestCompletionOptions
                    };
                    return await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "EnableAutoComplete", OperationType.Update);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>> ListIterationsAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitPullRequestIteration> result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    List<GitPullRequestIteration> list = await _gitHttpClient.GetPullRequestIterationsAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken);
                    return (IReadOnlyList<GitPullRequestIteration>)list;
                }, "ListIterations", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestIterationChanges>> GetIterationChangesAsync(string repositoryId, int pullRequestId, int iteration, CancellationToken cancellationToken = default)
        {
            try
            {
                GitPullRequestIterationChanges changes = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.GetPullRequestIterationChangesAsync(
                        repositoryId: repositoryId,
                        pullRequestId: pullRequestId,
                        iterationId: iteration,
                        project: ProjectName,
                        cancellationToken: cancellationToken);
                }, "GetIterationChanges", OperationType.Read);

                return AzureDevOpsActionResult<GitPullRequestIterationChanges>.Success(changes, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestIterationChanges>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> AddReviewerAsync(string repositoryId, int pullRequestId, (string localId, string name) reviewer, CancellationToken cancellationToken = default)
        {
            string error = string.Empty;
            if(string.IsNullOrWhiteSpace(reviewer.localId))
                return AzureDevOpsActionResult<bool>.Failure("Reviewer localId must be provided", Logger);
            var reviewerPayload = new
            {
                localId = reviewer.localId,
                DisplayName = reviewer.name ?? string.Empty
            };

            try
            {
                bool added = await ExecuteWithExceptionHandlingAsync(async () =>

                {
                    string requestUrl = $"{OrganizationUrl}/{ProjectName}/_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/reviewers/{reviewer.localId}?api-version={GlobalConstants.ApiVersion}";

                    string content = JsonSerializer.Serialize(reviewerPayload);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    using HttpResponseMessage response = await _httpClient.PutAsync(requestUrl, httpContent, cancellationToken);
                    if(!response.IsSuccessStatusCode)
                    {
                        error = await response.Content.ReadAsStringAsync(cancellationToken);
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }, "AddReviewer", OperationType.Update);

                return added ? AzureDevOpsActionResult<bool>.Success(added, Logger) : AzureDevOpsActionResult<bool>.Failure(error, Logger);

            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, error, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers, CancellationToken cancellationToken = default)
        {
            if(reviewers == null || reviewers.Length == 0)
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

        public Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote) =>
            SetReviewerVoteAsync(repositoryId, pullRequestId, reviewerId, vote, CancellationToken.None);

        public async Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote, CancellationToken cancellationToken)
        {
            try
            {
                IdentityRefWithVote result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var reviewerUpdate = new IdentityRefWithVote { Id = reviewerId, Vote = vote };
                    return await _gitHttpClient.CreatePullRequestReviewerAsync(
                    reviewer: reviewerUpdate,
                    repositoryId: repositoryId,
                    reviewerId: reviewerId,
                    pullRequestId: pullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "SetReviewerVote", OperationType.Update);

                return AzureDevOpsActionResult<IdentityRefWithVote>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IdentityRefWithVote>.Failure(ex, Logger);
            }
        }

        public Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds) =>
            RemoveReviewersAsync(repositoryId, pullRequestId, CancellationToken.None, reviewerIds);

        public async Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, CancellationToken cancellationToken, params String[] reviewerIds)
        {
            try
            {
                bool success = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    foreach(string id in reviewerIds)
                        await _gitHttpClient.DeletePullRequestReviewerAsync(
                         repositoryId: repositoryId,
                         reviewerId: id,
                         pullRequestId: pullRequestId,
                         project: ProjectName);
                    return true;
                }, "RemoveReviewers", OperationType.Delete);

                return AzureDevOpsActionResult<bool>.Success(success, Logger);
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
                int id = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var comment = new Comment { Content = commentThreadOptions.Comment, CommentType = CommentType.Text };
                    var commentThread = new GitPullRequestCommentThread { Comments = [comment], Status = CommentThreadStatus.Active };
                    if(!string.IsNullOrWhiteSpace(commentThreadOptions.FilePath))
                    {
                        commentThread.ThreadContext = new CommentThreadContext
                        {
                            FilePath = commentThreadOptions.FilePath,
                            RightFileStart = new CommentPosition { Line = 1, Offset = 1 },
                            RightFileEnd = new CommentPosition { Line = 1, Offset = 1 }
                        };
                        if(commentThreadOptions.IsLeftSide)
                        {
                            commentThread.ThreadContext.LeftFileStart = commentThread.ThreadContext.LeftFileEnd = new CommentPosition { Line = 1, Offset = 1 };
                        }
                    }
                    GitPullRequestCommentThread created = await _gitHttpClient.CreateThreadAsync(
                    commentThread: commentThread,
                    repositoryId: commentThreadOptions.RepositoryId,
                    pullRequestId: commentThreadOptions.PullRequestId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                    return created.Id;
                }, "CreateCommentThread", OperationType.Create);

                return AzureDevOpsActionResult<int>.Success(id, Logger);
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
                int id = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    Comment newComment = new Comment { Content = commentReplyOptions.Comment, CommentType = CommentType.Text };
                    GitPullRequestCommentThread gitPullRequestCommentThread;
                    if(commentReplyOptions.ResolveThread)
                    {
                        gitPullRequestCommentThread = new GitPullRequestCommentThread { Id = commentReplyOptions.ThreadId, Comments = [newComment], Status = CommentThreadStatus.Fixed };
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
                        await _gitHttpClient.CreateCommentAsync(newComment, commentReplyOptions.Repository, commentReplyOptions.PullRequestId, commentReplyOptions.ThreadId, ProjectName, cancellationToken);
                        gitPullRequestCommentThread = await _gitHttpClient.GetPullRequestThreadAsync(
                            repositoryId: commentReplyOptions.Repository,
                            pullRequestId: commentReplyOptions.PullRequestId,
                            threadId: commentReplyOptions.ThreadId,
                            project: ProjectName,
                            cancellationToken: cancellationToken);
                    }
                    return gitPullRequestCommentThread.Id;
                }, "ReplyToCommentThread", OperationType.Update);

                return AzureDevOpsActionResult<int>.Success(id, Logger);
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
                Comment result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var updatedComment = new Comment { Content = commentEditOptions.NewContent };
                    return await _gitHttpClient.UpdateCommentAsync(
                    comment: updatedComment,
                    repositoryId: commentEditOptions.Repository,
                    pullRequestId: commentEditOptions.PullRequest,
                    threadId: commentEditOptions.ThreadId,
                    commentId: commentEditOptions.CommentId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "EditComment", OperationType.Update);

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
                bool deleted = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    await _gitHttpClient.DeleteCommentAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId,
                    commentId: commentId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                    return true;
                }, "DeleteComment", OperationType.Delete);

                return AzureDevOpsActionResult<bool>.Success(deleted, Logger);
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
                IReadOnlyList<GitPullRequestCommentThread> threads = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    List<GitPullRequestCommentThread> list = await _gitHttpClient.GetThreadsAsync(
                        repositoryId: repositoryId,
                        pullRequestId: pullRequestId,
                        project: ProjectName,
                        cancellationToken: cancellationToken);
                    return (IReadOnlyList<GitPullRequestCommentThread>)list;
                }, "ListPullRequestThreads", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>.Success(threads, Logger);
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
                IReadOnlyList<Comment> comments = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    List<Comment> list = await _gitHttpClient.GetCommentsAsync(
                        repositoryId: repositoryId,
                        pullRequestId: pullRequestId,
                        threadId: threadId,
                        project: ProjectName,
                        cancellationToken: cancellationToken);
                    return (IReadOnlyList<Comment>)list;
                }, "ListPullRequestThreadComments", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<Comment>>.Success(comments, Logger);
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
                GitPullRequestCommentThread threadUpdate = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var commentThread = new GitPullRequestCommentThread { Status = CommentThreadStatus.Fixed };
                    return await _gitHttpClient.UpdateThreadAsync(
                    commentThread: commentThread,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "ResolveCommentThread", OperationType.Update);

                return AzureDevOpsActionResult<GitPullRequestCommentThread>.Success(threadUpdate, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestCommentThread>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, CancellationToken cancellationToken = default, params string[] labels)
        {
            try
            {
                IList<WebApiTagDefinition> result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var added = new List<WebApiTagDefinition>();
                    foreach(string label in labels.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        var tagRequestData = new WebApiCreateTagRequestData { Name = label };
                        WebApiTagDefinition def = await _gitHttpClient.CreatePullRequestLabelAsync(
                        label: tagRequestData,
                        project: ProjectName,
                        repositoryId: repository,
                        pullRequestId: pullRequestId,
                        cancellationToken: cancellationToken
                    );
                        added.Add(def);
                    }
                    return (IList<WebApiTagDefinition>)added;
                }, "AddLabels", OperationType.Update);

                return AzureDevOpsActionResult<IList<WebApiTagDefinition>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IList<WebApiTagDefinition>>.Failure(ex, Logger);
            }
        }

        public Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels) =>
            AddLabelsAsync(repository, pullRequestId, CancellationToken.None, labels);

        public async Task<AzureDevOpsActionResult<bool>> RemoveLabelAsync(string repositoryId, int pullRequestId, string label, CancellationToken cancellationToken = default)
        {
            try
            {
                bool removed = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    await _gitHttpClient.DeletePullRequestLabelsAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    labelIdOrName: label,
                    cancellationToken: cancellationToken
                );
                    return true;
                }, "RemoveLabel", OperationType.Delete);

                return AzureDevOpsActionResult<bool>.Success(removed, Logger);
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
                IReadOnlyList<WebApiTagDefinition> labels = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    List<WebApiTagDefinition> list = await _gitHttpClient.GetPullRequestLabelsAsync(
                    project: ProjectName,
                    repositoryId: repository,
                    pullRequestId: pullRequestId,
                    cancellationToken: cancellationToken);
                    return (IReadOnlyList<WebApiTagDefinition>)list;
                }, "GetPullRequestLabels", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>.Success(labels, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>.Failure(ex, Logger);
            }
        }
    }
}


