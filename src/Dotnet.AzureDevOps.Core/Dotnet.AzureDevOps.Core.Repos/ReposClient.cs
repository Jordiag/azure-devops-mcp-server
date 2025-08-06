using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public class ReposClient : IReposClient
    {
        private readonly string _projectName;
        private readonly GitHttpClient _gitHttpClient;
        private readonly string _organizationUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;

        public ReposClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _projectName = projectName;
            _organizationUrl = organizationUrl;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _gitHttpClient = connection.GetClient<GitHttpClient>();
            _httpClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };
            string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<AzureDevOpsActionResult<int>> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions)
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
                    project: _projectName
                );

                return AzureDevOpsActionResult<int>.Success(createdPR.PullRequestId, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId)
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
                    project: _projectName
                );

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> GetPullRequestAsync(string repositoryId, int pullRequestId)
        {
            try
            {
                GitPullRequest pr = await _gitHttpClient.GetPullRequestAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId
                );
                return AzureDevOpsActionResult<GitPullRequest>.Success(pr, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> CompletePullRequestAsync(
            string repositoryId,
            int pullRequestId,
            bool squashMerge = false,
            bool deleteSourceBranch = false,
            GitCommitRef? lastMergeSourceCommit = null,
            string? commitMessage = null)
        {
            try
            {
                GitPullRequest pullRequestUpdate = new GitPullRequest
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
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId
                );

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> UpdatePullRequestAsync(
            string repositoryId,
            int pullRequestId,
            PullRequestUpdateOptions pullRequestUpdateOptions)
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
                    project: _projectName);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsAsync(
            string repositoryId, PullRequestSearchOptions pullRequestSearchOptions)
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
                    project: _projectName);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(pullRequests, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> AddReviewerAsync(string repositoryId, int pullRequestId, (string localId, string name) reviewer)
        {
            if(string.IsNullOrEmpty(reviewer.localId))
                return AzureDevOpsActionResult<bool>.Failure("Reviewer localId must be provided", _logger);

            var reviewerPayload = new
            {
                localId = reviewer.localId,
                DisplayName = reviewer.name ?? string.Empty
            };

            string requestUrl = $"{_organizationUrl}/{_projectName}/_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/reviewers/{reviewer.localId}?api-version={GlobalConstants.ApiVersion}";

            string content = JsonSerializer.Serialize(reviewerPayload);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = await _httpClient.PutAsync(requestUrl, httpContent);
                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
                }
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> AddReviewersAsync(string repositoryId, int pullRequestId, (string localId, string name)[] reviewers)
        {
            if(reviewers is null || reviewers.Length == 0)
                return AzureDevOpsActionResult<bool>.Failure("No reviewers specified", _logger);

            bool allAdded = true;
            foreach((string localId, string name) reviewer in reviewers)
            {
                AzureDevOpsActionResult<bool> result = await AddReviewerAsync(repositoryId, pullRequestId, reviewer);
                if(!result.IsSuccessful || result.Value != true)
                    allAdded = false;
            }

            return allAdded
                ? AzureDevOpsActionResult<bool>.Success(true, _logger)
                : AzureDevOpsActionResult<bool>.Failure("One or more reviewers could not be added", _logger);
        }

        public async Task<AzureDevOpsActionResult<IdentityRefWithVote>> SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote)
        {
            try
            {
                IdentityRefWithVote reviewerUpdate = new IdentityRefWithVote
                {
                    Id = reviewerId,
                    Vote = vote
                };

                IdentityRefWithVote result = await _gitHttpClient.CreatePullRequestReviewerAsync(
                    reviewer: reviewerUpdate,
                    repositoryId: repositoryId,
                    reviewerId: reviewerId,
                    pullRequestId: pullRequestId,
                    project: _projectName);

                return AzureDevOpsActionResult<IdentityRefWithVote>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IdentityRefWithVote>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<int>> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions)
        {
            try
            {
                Comment comment = new Comment
                {
                    Content = commentThreadOptions.Comment,
                    CommentType = CommentType.Text
                };

                GitPullRequestCommentThread commentThread = new GitPullRequestCommentThread
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
                    project: _projectName);

                return AzureDevOpsActionResult<int>.Success(created.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<int>> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions)
        {
            try
            {
                Comment newComment = new Comment
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
                        project: _projectName);
                }
                else
                {
                    Comment commentResult = await _gitHttpClient.CreateCommentAsync(
                        comment: newComment,
                        repositoryId: commentReplyOptions.Repository,
                        pullRequestId: commentReplyOptions.PullRequestId,
                        threadId: commentReplyOptions.ThreadId,
                        project: _projectName);

                    gitPullRequestCommentThread = await _gitHttpClient.GetPullRequestThreadAsync(
                        repositoryId: commentReplyOptions.Repository,
                        pullRequestId: commentReplyOptions.PullRequestId,
                        threadId: commentReplyOptions.ThreadId,
                        project: _projectName);
                }

                return AzureDevOpsActionResult<int>.Success(gitPullRequestCommentThread.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<int>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IList<WebApiTagDefinition>>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels)
        {
            try
            {
                List<WebApiTagDefinition> webApiTagDefinitions = new List<WebApiTagDefinition>();
                foreach(string label in labels.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    WebApiCreateTagRequestData tagRequestData = new WebApiCreateTagRequestData { Name = label };

                    WebApiTagDefinition webApiTagDefinition = await _gitHttpClient.CreatePullRequestLabelAsync(
                        label: tagRequestData,
                        project: _projectName,
                        repositoryId: repository,
                        pullRequestId: pullRequestId
                    );

                    webApiTagDefinitions.Add(webApiTagDefinition);
                }

                return AzureDevOpsActionResult<IList<WebApiTagDefinition>>.Success(webApiTagDefinitions, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IList<WebApiTagDefinition>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> RemoveLabelAsync(string repositoryId, int pullRequestId, string label)
        {
            try
            {
                await _gitHttpClient.DeletePullRequestLabelsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    labelIdOrName: label
                );
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>> GetPullRequestLabelsAsync(string repository, int pullRequestId)
        {
            try
            {
                IReadOnlyList<WebApiTagDefinition> result = await _gitHttpClient.GetPullRequestLabelsAsync(
                    project: _projectName,
                    repositoryId: repository,
                    pullRequestId: pullRequestId);

                return AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds)
        {
            try
            {
                foreach(string id in reviewerIds)
                    await _gitHttpClient.DeletePullRequestReviewerAsync(
                        repositoryId: repositoryId,
                        reviewerId: id,
                        pullRequestId: pullRequestId,
                        project: _projectName);
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestStatus>> SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions)
        {
            try
            {
                GitPullRequestStatus status = new GitPullRequestStatus
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
                    project: _projectName);

                return AzureDevOpsActionResult<GitPullRequestStatus>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestStatus>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequest>> EnableAutoCompleteAsync(
            string repositoryId,
            int pullRequestId,
            string displayName,
            string localId,
            GitPullRequestCompletionOptions gitPullRequestCompletionOptions)
        {
            try
            {
                GitPullRequest pullRequestUpdate = new GitPullRequest
                {
                    AutoCompleteSetBy = new IdentityRef { DisplayName = displayName, Id = localId },
                    CompletionOptions = gitPullRequestCompletionOptions
                };

                GitPullRequest result = await _gitHttpClient.UpdatePullRequestAsync(
                    gitPullRequestToUpdate: pullRequestUpdate,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    project: _projectName);

                return AzureDevOpsActionResult<GitPullRequest>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequest>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>> ListIterationsAsync(
            string repositoryId, int pullRequestId)
        {
            try
            {
                IReadOnlyList<GitPullRequestIteration> result = await _gitHttpClient.GetPullRequestIterationsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestIterationChanges>> GetIterationChangesAsync(
            string repositoryId, int pullRequestId, int iteration)
        {
            try
            {
                GitPullRequestIterationChanges result = await _gitHttpClient.GetPullRequestIterationChangesAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    iterationId: iteration,
                    project: _projectName);

                return AzureDevOpsActionResult<GitPullRequestIterationChanges>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestIterationChanges>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByLabelAsync(
            string repositoryId, string labelName, PullRequestStatus pullRequestStatus)
        {
            try
            {
                AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> allPullRequestsResult = await ListPullRequestsAsync(
                    repositoryId, new PullRequestSearchOptions { Status = pullRequestStatus });

                if(!allPullRequestsResult.IsSuccessful || allPullRequestsResult.Value == null)
                    return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(allPullRequestsResult.ErrorMessage ?? "Failed to list pull requests.", _logger);

                List<GitPullRequest> filtered = allPullRequestsResult.Value.Where(pullRequest => pullRequest.Labels.Any(label =>
                    string.Equals(label.Name, labelName, StringComparison.OrdinalIgnoreCase))).ToList();

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(filtered, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<List<GitRefUpdateResult>>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha)
        {
            try
            {
                GitRefUpdate refUpdate = new GitRefUpdate
                {
                    Name = newRefName,
                    OldObjectId = "0000000000000000000000000000000000000000",
                    NewObjectId = baseCommitSha
                };

                List<GitRefUpdateResult> result = await _gitHttpClient.UpdateRefsAsync(
                    refUpdates: new[] { refUpdate },
                    repositoryId: repositoryId,
                    project: _projectName);

                return AzureDevOpsActionResult<List<GitRefUpdateResult>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<List<GitRefUpdateResult>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitCommitDiffs>> GetCommitDiffAsync(
            string repositoryId, string baseSha, string targetSha)
        {
            try
            {
                GitBaseVersionDescriptor baseDesc = new GitBaseVersionDescriptor
                {
                    Version = baseSha,
                    VersionType = GitVersionType.Commit
                };

                GitTargetVersionDescriptor targetDesc = new GitTargetVersionDescriptor
                {
                    Version = targetSha,
                    VersionType = GitVersionType.Commit
                };

                GitCommitDiffs result = await _gitHttpClient.GetCommitDiffsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    baseVersionDescriptor: baseDesc,
                    targetVersionDescriptor: targetDesc);

                return result == null
                    ? AzureDevOpsActionResult<GitCommitDiffs>.Failure("No differences found between the specified commits.", _logger)
                    : AzureDevOpsActionResult<GitCommitDiffs>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitCommitDiffs>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<Guid>> CreateRepositoryAsync(string newRepositoryName)
        {
            try
            {
                GitRepositoryCreateOptions newRepositoryOptions = new GitRepositoryCreateOptions { Name = newRepositoryName };

                GitRepository repo = await _gitHttpClient.CreateRepositoryAsync(
                    gitRepositoryToCreate: newRepositoryOptions,
                    project: _projectName);

                return AzureDevOpsActionResult<Guid>.Success(repo.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteRepositoryAsync(Guid repositoryId)
        {
            try
            {
                await _gitHttpClient.DeleteRepositoryAsync(
                    repositoryId: repositoryId,
                    project: _projectName);
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryAsync(Guid repositoryId)
        {
            try
            {
                GitRepository repo = await _gitHttpClient.GetRepositoryAsync(
                    project: _projectName,
                    repositoryId: repositoryId);
                return AzureDevOpsActionResult<GitRepository>.Success(repo, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<Comment>> EditCommentAsync(CommentEditOptions commentEditOptions)
        {
            try
            {
                Comment update = new Comment { Content = commentEditOptions.NewContent };
                Comment result = await _gitHttpClient.UpdateCommentAsync(
                    comment: update,
                    repositoryId: commentEditOptions.Repository,
                    pullRequestId: commentEditOptions.PullRequest,
                    threadId: commentEditOptions.ThreadId,
                    commentId: commentEditOptions.CommentId,
                    project: _projectName);
                return AzureDevOpsActionResult<Comment>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Comment>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId)
        {
            try
            {
                await _gitHttpClient.DeleteCommentAsync(
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId,
                    commentId: commentId,
                    project: _projectName);
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitAnnotatedTag>> CreateTagAsync(TagCreateOptions tagCreateOptions)
        {
            try
            {
                GitAnnotatedTag annotatedTag = new GitAnnotatedTag
                {
                    Name = tagCreateOptions.Name,
                    ObjectId = string.Empty,
                    TaggedObject = new GitObject
                    {
                        ObjectId = tagCreateOptions.CommitSha,
                        ObjectType = GitObjectType.Commit
                    },
                    TaggedBy = new GitUserDate
                    {
                        Name = tagCreateOptions.TaggerName,
                        Email = tagCreateOptions.TaggerEmail,
                        Date = tagCreateOptions.Date.UtcDateTime
                    },
                    Message = tagCreateOptions.Message
                };

                GitAnnotatedTag gitAnnotatedTag = await _gitHttpClient.CreateAnnotatedTagAsync(
                    tagObject: annotatedTag,
                    project: _projectName,
                    repositoryId: tagCreateOptions.Repository
                );

                return AzureDevOpsActionResult<GitAnnotatedTag>.Success(gitAnnotatedTag, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitAnnotatedTag>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitAnnotatedTag>> GetTagAsync(
            string repositoryId, string objectId)
        {
            try
            {
                GitAnnotatedTag result = await _gitHttpClient.GetAnnotatedTagAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    objectId: objectId);
                return AzureDevOpsActionResult<GitAnnotatedTag>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitAnnotatedTag>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRefUpdateResult>> DeleteTagAsync(string repositoryId, string tagName)
        {
            try
            {
                string fullRefName = $"refs/tags/{tagName}";

                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    project: _projectName,
                    repositoryId: repositoryId);

                GitRef? tagRef = refs.FirstOrDefault(r => r.Name.Equals($"refs/tags/{tagName}", StringComparison.OrdinalIgnoreCase)) ??
                    throw new InvalidOperationException($"Tag '{tagName}' does not exist in repositoryId '{repositoryId}'.");

                GitRefUpdate refUpdate = new GitRefUpdate
                {
                    Name = fullRefName,
                    OldObjectId = tagRef.ObjectId,
                    NewObjectId = "0000000000000000000000000000000000000000"
                };

                List<GitRefUpdateResult> gitRefUpdateResultList = await _gitHttpClient.UpdateRefsAsync(
                    refUpdates: [refUpdate],
                    repositoryId: repositoryId,
                    project: _projectName);

                return AzureDevOpsActionResult<GitRefUpdateResult>.Success(gitRefUpdateResultList.First(r => r.OldObjectId == tagRef.ObjectId), _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRefUpdateResult>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1)
        {
            try
            {
                GitQueryCommitsCriteria criteria = new GitQueryCommitsCriteria
                {
                    ItemVersion = new GitVersionDescriptor
                    {
                        Version = branchName,
                        VersionType = GitVersionType.Branch
                    }
                };

                IReadOnlyList<GitCommitRef> result = await _gitHttpClient.GetCommitsAsync(
                    project: projectName,
                    repositoryId: repositoryName,
                    searchCriteria: criteria,
                    top: top
                );

                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRepository>>> ListRepositoriesAsync()
        {
            try
            {
                IReadOnlyList<GitRepository> result = await _gitHttpClient.GetRepositoriesAsync(project: _projectName);
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>> ListPullRequestsByProjectAsync(PullRequestSearchOptions pullRequestSearchOptions)
        {
            try
            {
                GitPullRequestSearchCriteria criteria = new GitPullRequestSearchCriteria
                {
                    Status = pullRequestSearchOptions.Status,
                    TargetRefName = pullRequestSearchOptions.TargetBranch,
                    SourceRefName = pullRequestSearchOptions.SourceBranch
                };

                List<GitPullRequest> pullRequests = await _gitHttpClient.GetPullRequestsByProjectAsync(
                    project: _projectName,
                    searchCriteria: criteria,
                    top: 1000
                );

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Success(pullRequests, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListBranchesAsync(string repositoryId)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    filter: "heads/",
                    includeLinks: true,
                    includeStatuses: null,
                    includeMyBranches: false,
                    latestStatusesOnly: null,
                    peelTags: null,
                    filterContains: null
                );

                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Success(refs, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListMyBranchesAsync(string repositoryId)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    includeLinks: true,
                    includeStatuses: null,
                    includeMyBranches: true,
                    latestStatusesOnly: null,
                    peelTags: null,
                    filterContains: null
                );

                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Success(refs, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>> ListPullRequestThreadsAsync(string repositoryId, int pullRequestId)
        {
            try
            {
                IReadOnlyList<GitPullRequestCommentThread> result = await _gitHttpClient.GetThreadsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    iteration: null,
                    baseIteration: null);

                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<Comment>>> ListPullRequestThreadCommentsAsync(string repositoryId, int pullRequestId, int threadId)
        {
            try
            {
                IReadOnlyList<Comment> result = await _gitHttpClient.GetCommentsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId);

                return AzureDevOpsActionResult<IReadOnlyList<Comment>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<Comment>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryByNameAsync(string repositoryName)
        {
            try
            {
                GitRepository repo = await _gitHttpClient.GetRepositoryAsync(
                    project: _projectName,
                    repositoryId: repositoryName
                );
                return AzureDevOpsActionResult<GitRepository>.Success(repo, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRef>> GetBranchAsync(string repositoryId, string branchName)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    filter: $"heads/{branchName}",
                    includeLinks: true,
                    includeStatuses: null,
                    includeMyBranches: null,
                    latestStatusesOnly: null,
                    peelTags: null,
                    filterContains: null
                );

                return refs == null || refs.Count == 0
                    ? AzureDevOpsActionResult<GitRef>.Failure($"Branch '{branchName}' not found in repository '{repositoryId}'.", _logger)
                    : AzureDevOpsActionResult<GitRef>.Success(refs[0], _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRef>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitPullRequestCommentThread>> ResolveCommentThreadAsync(string repositoryId, int pullRequestId, int threadId)
        {
            try
            {
                GitPullRequestCommentThread update = new GitPullRequestCommentThread
                {
                    Id = threadId,
                    Status = CommentThreadStatus.Fixed
                };

                GitPullRequestCommentThread result = await _gitHttpClient.UpdateThreadAsync(
                    commentThread: update,
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId,
                    threadId: threadId
                );

                return AzureDevOpsActionResult<GitPullRequestCommentThread>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitPullRequestCommentThread>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100)
        {
            try
            {
                IReadOnlyList<GitCommitRef> result = await _gitHttpClient.GetCommitsAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    searchCriteria: searchCriteria,
                    top: top
                );

                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<string>> CommitAddFileAsync(FileCommitOptions fileCommitOptions)
        {
            try
            {
                AzureDevOpsActionResult<GitRef> branchResult = await GetBranchAsync(fileCommitOptions.RepositoryName, fileCommitOptions.BranchName);
                if(!branchResult.IsSuccessful || branchResult.Value == null)
                    return AzureDevOpsActionResult<string>.Failure(branchResult.ErrorMessage ?? $"Branch '{fileCommitOptions.BranchName}' not found in repository '{fileCommitOptions.RepositoryName}'.", _logger);

                GitRef branch = branchResult.Value;

                GitChange change = new GitChange
                {
                    ChangeType = VersionControlChangeType.Add,
                    Item = new GitItem
                    {
                        Path = fileCommitOptions.FilePath
                    },
                    NewContent = new ItemContent
                    {
                        Content = fileCommitOptions.Content,
                        ContentType = ItemContentType.RawText
                    }
                };

                GitCommitRef commit = new GitCommitRef
                {
                    Comment = fileCommitOptions.CommitMessage,
                    Changes = [change]
                };

                GitRefUpdate referenceUpdate = new GitRefUpdate
                {
                    Name = $"refs/heads/{fileCommitOptions.BranchName}",
                    OldObjectId = branch.ObjectId
                };

                GitPush push = new GitPush
                {
                    RefUpdates = [referenceUpdate],
                    Commits = [commit]
                };

                GitPush result = await _gitHttpClient.CreatePushAsync(
                    push,
                    project: _projectName,
                    repositoryId: fileCommitOptions.RepositoryName,
                    userState: null
                );

                GitCommitRef pushedCommit = result.Commits.Last();
                return AzureDevOpsActionResult<string>.Success(pushedCommit.CommitId, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex, _logger);
            }
        }
    }
}