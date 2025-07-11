using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public class ReposClient : IReposClient
    {
        private readonly string _projectName;
        private readonly GitHttpClient _gitHttpClient;

        public ReposClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _projectName = projectName;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(organizationUrl), credentials);
            _gitHttpClient = connection.GetClient<GitHttpClient>();
        }

        /// <summary>
        /// Creates a pull request with the specified options, returning the PR ID if successful.
        /// </summary>
        public async Task<int?> CreatePullRequestAsync(PullRequestCreateOptions pullRequestCreateOptions)
        {
            var newPullRequest = new GitPullRequest
            {
                Title = pullRequestCreateOptions.Title,
                Description = pullRequestCreateOptions.Description,
                SourceRefName = pullRequestCreateOptions.SourceBranch, // must be in "refs/heads/..." form
                TargetRefName = pullRequestCreateOptions.TargetBranch,
                IsDraft = pullRequestCreateOptions.IsDraft
            };

            GitPullRequest createdPR = await _gitHttpClient.CreatePullRequestAsync(
                gitPullRequestToCreate: newPullRequest,
                repositoryId: pullRequestCreateOptions.RepositoryIdOrName,  // e.g. "MyRepo" or GUID
                project: _projectName
            );

            return createdPR.PullRequestId;
        }

        /// <summary>
        /// Abandons (effectively closes) an existing pull request by ID.
        /// </summary>
        public async Task AbandonPullRequestAsync(string repositoryIdOrName, int pullRequestId)
        {
            var pullRequestUpdate = new GitPullRequest
            {
                Status = PullRequestStatus.Abandoned
            };

            await _gitHttpClient.UpdatePullRequestAsync(
                gitPullRequestToUpdate: pullRequestUpdate,
                repositoryId: repositoryIdOrName,
                pullRequestId: pullRequestId,
                project: _projectName
            );
        }

        #region READ

        public async Task<GitPullRequest?> GetPullRequestAsync(string repositoryId, int pullRequestId)
        {
            try
            {
                return await _gitHttpClient.GetPullRequestAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    pullRequestId: pullRequestId
                    );
            }
            catch(VssServiceException)
            {
                return null;
            }
        }

        #endregion

        #region COMPLETE / MERGE

        public async Task CompletePullRequestAsync(
            string repositoryId,
            int pullRequestId,
            bool squashMerge = false,
            GitCommitRef? lastMergeSourceCommit = null,
            string? commitMessage = null)
        {
            var pullRequestUpdate = new GitPullRequest
            {
                LastMergeSourceCommit = lastMergeSourceCommit,
                Status = PullRequestStatus.Completed,
                CompletionOptions = new GitPullRequestCompletionOptions
                {
                    SquashMerge = squashMerge,
                    DeleteSourceBranch = true,
                    MergeCommitMessage = commitMessage
                }
            };


            await _gitHttpClient.UpdatePullRequestAsync(
                gitPullRequestToUpdate: pullRequestUpdate,
                project: _projectName,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId
                );
        }

        #endregion

        public async Task UpdatePullRequestAsync(
            string repositoryId,
            int pullRequestId,
            PullRequestUpdateOptions pullRequestUpdateOptions)
        {
            var pullRequestUpdate = new GitPullRequest
            {
                Title = pullRequestUpdateOptions.Title,
                Description = pullRequestUpdateOptions.Description,
                IsDraft = pullRequestUpdateOptions.IsDraft
            };

            if(pullRequestUpdateOptions.ReviewerIds is { } reviewers)
                pullRequestUpdate.Reviewers = [.. reviewers.Select(id => new IdentityRefWithVote { Id = id })];

            await _gitHttpClient.UpdatePullRequestAsync(
                gitPullRequestToUpdate: pullRequestUpdate,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                project: _projectName);
        }


        #region LIST / QUERY

        public async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsAsync(
            string repositoryId, PullRequestSearchOptions pullRequestSearchOptions)
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

            return pullRequests;
        }

        #endregion

        #region REVIEWERS

        public async Task AddReviewersAsync(string repositoryId, int pullRequestId, (string guid, string name)[] reviewerInfos)
        {
            if(reviewerInfos.Length == 0)
                return;

            IdentityRef[] reviewers = [.. reviewerInfos
                .Select(info => new IdentityRef
                {
                    Id = info.guid
                })];

            await _gitHttpClient.CreatePullRequestReviewersAsync(
                reviewers: reviewers,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                project: _projectName);
        }

        public async Task SetReviewerVoteAsync(string repositoryId, int pullRequestId, string reviewerId, short vote)
        {
            var reviewerUpdate = new IdentityRefWithVote
            {
                Id = reviewerId,
                Vote = vote
            };

            await _gitHttpClient.CreatePullRequestReviewerAsync(
                reviewer: reviewerUpdate, 
                repositoryId: repositoryId, 
                reviewerId: reviewerId, 
                pullRequestId: pullRequestId, 
                project: _projectName);
        }

        #endregion

        #region COMMENT THREADS

        public async Task<int> CreateCommentThreadAsync(CommentThreadOptions commentThreadOptions)
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
                project: _projectName);

            return created.Id;
        }

        public async Task<int?> ReplyToCommentThreadAsync(CommentReplyOptions commentReplyOptions)
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
                    project: _projectName);
            }
            else
            {
                _ = await _gitHttpClient.CreateCommentAsync(
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

            return gitPullRequestCommentThread.Id;
        }


        #endregion

        #region LABELS / TAGS

        public async Task<IList<WebApiTagDefinition>> AddLabelsAsync(string repository, int pullRequestId, params string[] labels)
        {
            var webApiTagDefinitions = new List<WebApiTagDefinition>();
            foreach(string label in labels.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var tagRequestData = new WebApiCreateTagRequestData { Name = label };

                WebApiTagDefinition webApiTagDefinition = await _gitHttpClient.CreatePullRequestLabelAsync(
                    label: tagRequestData,
                     project: _projectName,
                    repositoryId: repository,
                    pullRequestId: pullRequestId
                   );

                webApiTagDefinitions.Add(webApiTagDefinition);
            }

            return webApiTagDefinitions;
        }

        public async Task RemoveLabelAsync(string repositoryId, int pullRequestId, string label) => 
            await _gitHttpClient.DeletePullRequestLabelsAsync(
                project: _projectName,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                labelIdOrName: label
                );

        public async Task<IReadOnlyList<WebApiTagDefinition>> GetPullRequestLabelsAsync(string repository, int pullRequestId) =>
            await _gitHttpClient.GetPullRequestLabelsAsync(
                project: _projectName,
                repositoryId: repository,
                pullRequestId: pullRequestId);


        #endregion

        #region REVIEWERS – remove
        public async Task RemoveReviewersAsync(string repositoryId, int pullRequestId, params string[] reviewerIds)
        {
            foreach(string id in reviewerIds)
                await _gitHttpClient.DeletePullRequestReviewerAsync(
                    repositoryId: repositoryId,
                    reviewerId: id,
                    pullRequestId: pullRequestId,
                    project: _projectName);
        }
        #endregion



        #region STATUS CHECKS
        public async Task SetPullRequestStatusAsync(string repositoryId, int pullRequestId, PullRequestStatusOptions pullRequestStatusOptions)
        {
            var status = new GitPullRequestStatus
            {
                Context = new GitStatusContext { Name = pullRequestStatusOptions.ContextName, Genre = pullRequestStatusOptions.ContextGenre },
                State = pullRequestStatusOptions.State,
                Description = pullRequestStatusOptions.Description,
                TargetUrl = pullRequestStatusOptions.TargetUrl
            };

            await _gitHttpClient.CreatePullRequestStatusAsync(
                status: status,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                project: _projectName);
        }
        #endregion



        #region AUTO-COMPLETE
        public async Task EnableAutoCompleteAsync(
            string repositoryId,
            int pullRequestId,
            GitPullRequestCompletionOptions gitPullRequestCompletionOptions)
        {
            var pullRequestUpdate = new GitPullRequest
            {
                AutoCompleteSetBy = new IdentityRef { DisplayName = "bot" },
                CompletionOptions = gitPullRequestCompletionOptions
            };

            await _gitHttpClient.UpdatePullRequestAsync(
                gitPullRequestToUpdate: pullRequestUpdate,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                project: _projectName);
        }
        #endregion



        #region ITERATIONS / DIFF
        public async Task<IReadOnlyList<GitPullRequestIteration>> ListIterationsAsync(
            string repositoryId, int pullRequestId) =>
            await _gitHttpClient.GetPullRequestIterationsAsync(
                project: _projectName,
                repositoryId: repositoryId,
                pullRequestId: pullRequestId);

        public async Task<GitPullRequestIterationChanges> GetIterationChangesAsync(
            string repositoryId, int pullRequestId, int iteration) => 
            await _gitHttpClient.GetPullRequestIterationChangesAsync(
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                iterationId: iteration,
                project: _projectName);
        #endregion



        #region PRs by LABEL (client-side filter)
        public async Task<IReadOnlyList<GitPullRequest>> ListPullRequestsByLabelAsync(
            string repositoryId, string labelName, PullRequestStatus pullRequestStatus)
        {
            IReadOnlyList<GitPullRequest> allPullRequests = await ListPullRequestsAsync(
                repositoryId, new PullRequestSearchOptions { Status = pullRequestStatus });

            return [.. allPullRequests.Where(pullRequest => pullRequest.Labels.Any(label =>
                string.Equals(label.Name, labelName, StringComparison.OrdinalIgnoreCase)))];
        }
        #endregion



        #region BRANCH / COMMIT helpers
        public async Task CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha)
        {
            var refUpdate = new GitRefUpdate
            {
                Name = newRefName,
                OldObjectId = "0000000000000000000000000000000000000000",
                NewObjectId = baseCommitSha
            };

            var push = new GitPush
            {
                RefUpdates = [refUpdate],
                Commits = []
            };

            await _gitHttpClient.CreatePushAsync(
                push: push,
                repositoryId: repositoryId,
                project: _projectName);
        }

        public async Task<GitCommitDiffs> GetCommitDiffAsync(
            string repositoryId, string baseSha, string targetSha)
        {
            var baseDesc = new GitBaseVersionDescriptor
            {
                Version = baseSha,
                VersionType = GitVersionType.Commit
            };

            var targetDesc = new GitTargetVersionDescriptor
            {
                Version = targetSha,
                VersionType = GitVersionType.Commit
            };

            return await _gitHttpClient.GetCommitDiffsAsync(
                project: _projectName,
                repositoryId: repositoryId,
                baseVersionDescriptor: baseDesc,
                targetVersionDescriptor: targetDesc);
        }


        public async Task<Guid> CreateRepositoryAsync(string newRepositoryName)
        {
            var newRepositoryOptions = new GitRepositoryCreateOptions { Name = newRepositoryName };

            GitRepository repo = await _gitHttpClient.CreateRepositoryAsync(
                gitRepositoryToCreate: newRepositoryOptions,
                project: _projectName);

            return repo.Id;
        }

        public async Task DeleteRepositoryAsync(Guid repositoryId) =>
            await _gitHttpClient.DeleteRepositoryAsync(
                repositoryId: repositoryId,
                project: _projectName);

        public async Task<GitRepository?> GetRepositoryAsync(Guid repositoryId)
        {
            try
            {
                return await _gitHttpClient.GetRepositoryAsync(
                    project: _projectName,
                    repositoryId: repositoryId);
            }
            catch (VssServiceException)
            {
                return null;
            }
        }
        #endregion

        #region COMMENT edit / delete
        public async Task EditCommentAsync(CommentEditOptions commentEditOptions)
        {
            var update = new Comment { Content = commentEditOptions.NewContent };
            await _gitHttpClient.UpdateCommentAsync(
                comment: update,
                repositoryId: commentEditOptions.Repository,
                pullRequestId: commentEditOptions.PullRequest,
                threadId: commentEditOptions.ThreadId,
                commentId: commentEditOptions.CommentId,
                project: _projectName);
        }

        public async Task DeleteCommentAsync(string repositoryId, int pullRequestId, int threadId, int commentId) => 
            await _gitHttpClient.DeleteCommentAsync(
                repositoryId: repositoryId,
                pullRequestId: pullRequestId,
                threadId: threadId,
                commentId: commentId,
                project: _projectName);
        #endregion

        #region GIT TAGS  (Repos ▸ Tags UI)

        public async Task<GitAnnotatedTag> CreateTagAsync(TagCreateOptions tagCreateOptions)
        {
            /* ── annotated tag (shows message & tagger) ───────────────────── */
            var annotatedTag = new GitAnnotatedTag
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

            return gitAnnotatedTag;
        }

        public async Task<GitAnnotatedTag> GetTagAsync(
            string repositoryId, string objectId) => 
            await _gitHttpClient.GetAnnotatedTagAsync(
                project: _projectName,
                repositoryId: repositoryId,
                objectId: objectId);

        public async Task<GitRefUpdateResult?> DeleteTagAsync(string repositoryId, string tagName)
        {
            string fullRefName = $"refs/tags/{tagName}";

            List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                project: _projectName,
                repositoryId: repositoryId);
            
            GitRef? tagRef = refs.FirstOrDefault(r => r.Name.Equals($"refs/tags/{tagName}", StringComparison.OrdinalIgnoreCase)) ?? 
                throw new InvalidOperationException($"Tag '{tagName}' does not exist in repositoryId '{repositoryId}'.");

            var refUpdate = new GitRefUpdate
            {
                Name = fullRefName,
                OldObjectId = tagRef.ObjectId,
                NewObjectId = "0000000000000000000000000000000000000000"
            };

            List<GitRefUpdateResult> gitRefUpdateResultList = await _gitHttpClient.UpdateRefsAsync(
                refUpdates: [refUpdate],
                repositoryId: repositoryId,
                project: _projectName);

            return gitRefUpdateResultList.First(r => r.OldObjectId == tagRef.ObjectId);
        }
        #endregion

        public async Task<IReadOnlyList<GitCommitRef>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1)
        {
            var criteria = new GitQueryCommitsCriteria
            {
                ItemVersion = new GitVersionDescriptor
                {
                    Version = branchName,
                    VersionType = GitVersionType.Branch
                }
            };

            return await _gitHttpClient.GetCommitsAsync(
                project: projectName,
                repositoryId: repositoryName,
                searchCriteria: criteria,
                top: top
            );
        }
    }
}
