using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Repos.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Repos)]
    public class DotnetAzureDevOpsReposIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly ReposClient _reposClient;
        private readonly IdentityClient _identityClient;

        // Environment-driven settings
        private readonly string _repoName;
        private readonly string _srcBranch;
        private readonly string _targetBranch;
        private readonly string _userEmail;
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

        // Track created PRs so we can abandon if something fails
        private readonly List<int> _createdPrIds = [];

        public DotnetAzureDevOpsReposIntegrationTests(IntegrationTestFixture fixture)
        {
            _azureDevOpsConfiguration = fixture.Configuration;
            _repoName = _azureDevOpsConfiguration.RepoName ?? string.Empty;
            _srcBranch = _azureDevOpsConfiguration.SrcBranch;
            _targetBranch = _azureDevOpsConfiguration.TargetBranch;
            _userEmail = _azureDevOpsConfiguration.BotUserEmail;

            _reposClient = fixture.ReposClient;
            _identityClient = fixture.IdentityClient;
        }

        //[Fact]
        //public async Task CreateReadCompletePullRequest_SucceedsAsync()
        //{
        //    var createOptions = new PullRequestCreateOptions
        //    {
        //        ProjectIdOrName = _azureDevOpsConfiguration.ProjectName,
        //        RepositoryIdOrName = _repoName,
        //        Title = $"Advanced PR {UtcStamp()}",
        //        Description = "PR exercising advanced APIs",
        //        SourceBranch = _srcBranch,
        //        TargetBranch = _targetBranch,
        //        IsDraft = false
        //    };

        //    AzureDevOpsActionResult<GitRef> gitRefResult = await _reposClient.GetBranchAsync(_repoName, createOptions.SourceBranch);
        //    GitRef? gitRef = gitRefResult.Value;

        //    if(string.IsNullOrEmpty(gitRef?.Name))
        //    {
        //        AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommitsResult = await _reposClient.GetLatestCommitsAsync(
        //            _azureDevOpsConfiguration.ProjectName,
        //            _repoName,
        //            "main",
        //            top: 1);

        //        IReadOnlyList<GitCommitRef> latestCommits = latestCommitsResult.Value ?? [];
        //        if(latestCommits.Count == 0)
        //            return;

        //        string commitSha = latestCommits[0].CommitId;
        //        AzureDevOpsActionResult<List<GitRefUpdateResult>> branchResult = await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
        //        Assert.True(branchResult.IsSuccessful);
        //    }

        //    var pullRequestCreateOptions = new PullRequestCreateOptions
        //    {
        //        ProjectIdOrName = _azureDevOpsConfiguration.ProjectName,
        //        RepositoryIdOrName = _repoName,
        //        Title = $"Integration PR {DateTime.UtcNow:yyyyMMddHHmmss}",
        //        Description = "Created by automated test",
        //        SourceBranch = _srcBranch,
        //        TargetBranch = _targetBranch,
        //        IsDraft = false
        //    };

        //    AzureDevOpsActionResult<int> prIdResult = await _reposClient.CreatePullRequestAsync(pullRequestCreateOptions);
        //    Assert.True(prIdResult.IsSuccessful);
        //    int? pullRequestId = prIdResult.Value;
        //    Assert.True(pullRequestId.HasValue);
        //    _createdPrIds.Add(pullRequestId.Value);

        //    AzureDevOpsActionResult<GitPullRequest>? gtPullRequestResult = null;
        //    await WaitHelper.WaitUntilAsync(async () =>
        //    {
        //        gtPullRequestResult = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId.Value);
        //        return gtPullRequestResult.IsSuccessful && gtPullRequestResult.Value?.Status == PullRequestStatus.Active;
        //    }, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));

        //    GitPullRequest gtPullRequest = gtPullRequestResult!.Value!;

        //    await Task.Delay(1000);

        //    AzureDevOpsActionResult<GitPullRequest> completeResult = await _reposClient.CompletePullRequestAsync(
        //        _repoName,
        //        pullRequestId.Value,
        //        squashMerge: true,
        //        deleteSourceBranch: false,
        //        lastMergeSourceCommit: gtPullRequest.LastMergeSourceCommit);

        //    Assert.True(completeResult.IsSuccessful);

        //    AzureDevOpsActionResult<GitPullRequest>? prResult = null;
        //    await WaitHelper.WaitUntilAsync(async () =>
        //    {
        //        prResult = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId.Value);
        //        return prResult.IsSuccessful && prResult.Value?.Status == PullRequestStatus.Completed;
        //    }, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1));
        //}

        // TODO: Re-enable this test once the API is working again
        [Fact(Skip = "API not longer working")]
        public async Task ListAndReviewers_Workflow_SucceedsAsync()
        {
            var pullRequestCreateOptions = new PullRequestCreateOptions
            {
                ProjectIdOrName = _azureDevOpsConfiguration.ProjectName,
                RepositoryIdOrName = _repoName,
                Title = $"IT PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "PR created by integration test",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch,
                IsDraft = false
            };

            AzureDevOpsActionResult<GitRef> gitRefResult = await _reposClient.GetBranchAsync(_repoName, pullRequestCreateOptions.SourceBranch);
            GitRef? gitRef = gitRefResult.Value;

            if(string.IsNullOrEmpty(gitRef?.Name))
            {
                AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommitsResult = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repoName,
                    "main",
                    top: 1);

                IReadOnlyList<GitCommitRef> latestCommits = latestCommitsResult.Value ?? [];
                if(latestCommits.Count == 0)
                    return;

                string commitSha = latestCommits[0].CommitId;
                AzureDevOpsActionResult<List<GitRefUpdateResult>> branchResult = await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
                Assert.True(branchResult.IsSuccessful);
            }

            AzureDevOpsActionResult<int> prIdResult = await _reposClient.CreatePullRequestAsync(pullRequestCreateOptions);
            int pullRequestId = prIdResult.Value;
            _createdPrIds.Add(pullRequestId);

            (string localId, string displayName) reviewer = default;

            short voteValue = 10;

            if(!string.IsNullOrWhiteSpace(_userEmail))
            {
                AzureDevOpsActionResult<(string localId, string displayName)> reviewerResult = await _identityClient.GetUserLocalIdFromEmailAsync(_userEmail);

                if(string.IsNullOrWhiteSpace(reviewerResult.Value.localId) || string.IsNullOrWhiteSpace(reviewerResult.Value.displayName))
                {
                    throw new InvalidOperationException($"Could not find user with email {_userEmail} in Azure DevOps.");
                }

                (string localId, string displayName)[] reviewers = [(reviewerResult.Value.localId, reviewerResult.Value.displayName ?? string.Empty)];

                AzureDevOpsActionResult<bool> success = await _reposClient.AddReviewersAsync(_repoName, pullRequestId, reviewers);
                Assert.True(success.Value, "Failed to add reviewers to the pull request.");

                AzureDevOpsActionResult<GitPullRequest> prAfterReviewerResult = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId);
                GitPullRequest? prAfterReviewer = prAfterReviewerResult.Value;
                Assert.Contains(prAfterReviewer!.Reviewers, r => r.Id == reviewer.localId);

                await _reposClient.SetReviewerVoteAsync(_repoName, pullRequestId, reviewerResult.Value.localId, voteValue);
            }

            AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> listResult = await _reposClient.ListPullRequestsAsync(
                _repoName,
                new PullRequestSearchOptions { Status = PullRequestStatus.Active });

            IReadOnlyList<GitPullRequest> list = listResult.Value ?? [];
            Assert.Contains(list, p => p.PullRequestId == pullRequestId);
            Assert.Contains(list, p => p.Reviewers.Any(r => r.DisplayName == reviewer.displayName && r.Vote == voteValue));
        }

        //[Fact]
        //public async Task LabelsAndCommentsWorkflow_SucceedsAsync()
        //{
        //    // create PR
        //    var pullRequestCreateOptions = new PullRequestCreateOptions
        //    {
        //        ProjectIdOrName = _azureDevOpsConfiguration.ProjectName,
        //        RepositoryIdOrName = _repoName,
        //        Title = $"Tier2 PR {DateTime.UtcNow:yyyyMMddHHmmss}",
        //        Description = "Tier-2 labels/comments test",
        //        SourceBranch = _srcBranch,
        //        TargetBranch = _targetBranch,
        //        IsDraft = false
        //    };

        //    AzureDevOpsActionResult<int> prIdResult = await _reposClient.CreatePullRequestAsync(pullRequestCreateOptions);
        //    int pullRequestId = prIdResult.Value;
        //    _createdPrIds.Add(pullRequestId);

        //    /* ---------- LABELS ---------- */
        //    AzureDevOpsActionResult<IList<WebApiTagDefinition>> addLabelsResult = await _reposClient.AddLabelsAsync(_repoName, pullRequestId, "docs", "ready");
        //    Assert.True(addLabelsResult.IsSuccessful);

        //    AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>> webApiTagDefinitionsResult = await _reposClient.GetPullRequestLabelsAsync(_repoName, pullRequestId);
        //    IReadOnlyList<WebApiTagDefinition> webApiTagDefinitions = webApiTagDefinitionsResult.Value ?? [];
        //    Assert.Contains("docs", webApiTagDefinitions!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);
        //    Assert.Contains("ready", webApiTagDefinitions!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);

        //    AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> labelPullRequestsResult = await _reposClient.ListPullRequestsByLabelAsync(
        //        _repoName,
        //        "ready",
        //        PullRequestStatus.Active);
        //    IReadOnlyList<GitPullRequest> labelPullRequests = labelPullRequestsResult.Value ?? [];
        //    Assert.Contains(labelPullRequests, pr => pr.PullRequestId == pullRequestId);

        //    AzureDevOpsActionResult<bool> removeLabelResult = await _reposClient.RemoveLabelAsync(_repoName, pullRequestId, "docs");
        //    Assert.True(removeLabelResult.IsSuccessful);

        //    AzureDevOpsActionResult<IReadOnlyList<WebApiTagDefinition>> webApiTagDefinitions2Result = await _reposClient.GetPullRequestLabelsAsync(_repoName, pullRequestId);
        //    IReadOnlyList<WebApiTagDefinition> webApiTagDefinitions2 = webApiTagDefinitions2Result.Value ?? [];
        //    Assert.DoesNotContain("docs", webApiTagDefinitions2!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);

        //    AzureDevOpsActionResult<int> commentThreadIdResult = await _reposClient.CreateCommentThreadAsync(new CommentThreadOptions
        //    {
        //        RepositoryId = _repoName,
        //        PullRequestId = pullRequestId,
        //        Comment = "CI failed, please fix.",
        //        IsLeftSide = false
        //    });
        //    int commentThreadId = commentThreadIdResult.Value;

        //    AzureDevOpsActionResult<int> commentReplyIdResult = await _reposClient.ReplyToCommentThreadAsync(new CommentReplyOptions
        //    {
        //        Repository = _repoName,
        //        PullRequestId = pullRequestId,
        //        ThreadId = commentThreadId,
        //        Comment = "Fixed and repushed!",
        //        ResolveThread = true
        //    });
        //    int? commentReplyId = commentReplyIdResult.Value;

        //    Assert.NotNull(commentReplyId);

        //    AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>> threadsResult = await _reposClient.ListPullRequestThreadsAsync(_repoName, pullRequestId);
        //    IReadOnlyList<GitPullRequestCommentThread> threads = threadsResult.Value ?? [];
        //    Assert.Contains(threads, t => t.Id == commentThreadId);

        //    AzureDevOpsActionResult<IReadOnlyList<Comment>> commentsResult = await _reposClient.ListPullRequestThreadCommentsAsync(_repoName, pullRequestId, commentThreadId);
        //    IReadOnlyList<Comment> comments = commentsResult.Value ?? [];
        //    int commentId = comments.First().Id;

        //    AzureDevOpsActionResult<Comment> editCommentResult = await _reposClient.EditCommentAsync(new CommentEditOptions
        //    {
        //        Repository = _repoName,
        //        PullRequest = pullRequestId,
        //        ThreadId = commentThreadId,
        //        CommentId = commentId,
        //        NewContent = "Edited"
        //    });
        //    Assert.True(editCommentResult.IsSuccessful);

        //    AzureDevOpsActionResult<GitPullRequestCommentThread> resolveThreadResult = await _reposClient.ResolveCommentThreadAsync(_repoName, pullRequestId, commentThreadId);
        //    Assert.True(resolveThreadResult.IsSuccessful);

        //    AzureDevOpsActionResult<bool> deleteCommentResult = await _reposClient.DeleteCommentAsync(_repoName, pullRequestId, commentThreadId, commentId);
        //    Assert.True(deleteCommentResult.IsSuccessful);
        //}

        [Fact]
        public async Task Tags_CreateListDelete_Workflow_SucceedsAsync()
        {
            AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommitsResult = await _reposClient.GetLatestCommitsAsync(
                _azureDevOpsConfiguration.ProjectName,
                _repoName,
                _azureDevOpsConfiguration.MainBranchName);

            IReadOnlyList<GitCommitRef> latestCommits = latestCommitsResult.Value ?? [];
            string? latestCommitSha = latestCommits.Count > 0 ? latestCommits[0].CommitId : null;

            if(string.IsNullOrWhiteSpace(latestCommitSha))
                return;

            string annTag = $"it-ann-{UtcStamp()}";

            AzureDevOpsActionResult<GitAnnotatedTag> gitAnnotatedTagResult = await _reposClient.CreateTagAsync(new TagCreateOptions
            {
                Repository = _repoName,
                Name = annTag,
                CommitSha = latestCommitSha,
                Message = "Integration annotated tag",
                TaggerName = "integration-bot",
                TaggerEmail = "bot@example.com"
            });
            GitAnnotatedTag gitAnnotatedTag = gitAnnotatedTagResult.Value!;

            AzureDevOpsActionResult<GitAnnotatedTag> tagResult = await _reposClient.GetTagAsync(_repoName, gitAnnotatedTag.ObjectId);
            GitAnnotatedTag tag = tagResult.Value!;

            Assert.True(tag!.Name == annTag);

            AzureDevOpsActionResult<GitRefUpdateResult> deleteTagResult = await _reposClient.DeleteTagAsync(_repoName, annTag);

            GitRefUpdateResult? result = deleteTagResult.Value;
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task BranchListingAndDiff_SucceedsAsync()
        {
            AzureDevOpsActionResult<IReadOnlyList<GitRef>> branchesResult = await _reposClient.ListBranchesAsync(_repoName);
            IReadOnlyList<GitRef> branches = branchesResult.Value ?? [];
            Assert.NotEmpty(branches);

            AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> commitsResult = await _reposClient.GetLatestCommitsAsync(
                _azureDevOpsConfiguration.ProjectName,
                _repoName,
                _azureDevOpsConfiguration.MainBranchName,
                top: 2);

            IReadOnlyList<GitCommitRef> commits = commitsResult.Value ?? [];
            if(commits.Count < 2)
                return; // insufficient history

            string baseCommit = commits[1].CommitId;
            string targetCommit = commits[0].CommitId;

            AzureDevOpsActionResult<GitCommitDiffs> diffsResult = await _reposClient.GetCommitDiffAsync(_repoName, baseCommit, targetCommit);
            GitCommitDiffs diffs = diffsResult.Value!;
            Assert.NotNull(diffs);
        }

        // TODO: Re-enable this test once the vote functionality works in the pipeline.
        [Fact]
        public async Task AdvancedPullRequestWorkflow_SucceedsAsync()
        {
            /*
            var createOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"Advanced PR {UtcStamp()}",
                Description = "PR exercising advanced APIs",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            AzureDevOpsActionResult<GitRef> gitRefResult = await _reposClient.GetBranchAsync(_repoName, createOptions.SourceBranch);
            GitRef? gitRef = gitRefResult.Value;

            if(string.IsNullOrEmpty(gitRef?.Name))
            {
                AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommitsResult = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repoName,
                    "main",
                    top: 1);

                IReadOnlyList<GitCommitRef> latestCommits = latestCommitsResult.Value ?? [];
                if(latestCommits.Count == 0)
                    return;

                string commitSha = latestCommits[0].CommitId;
                AzureDevOpsActionResult<List<GitRefUpdateResult>> branchResult = await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
                Assert.True(branchResult.IsSuccessful);
            }

            var fileCommitOptions = new FileCommitOptions
            {
                RepositoryName = _azureDevOpsConfiguration.RepoName,
                BranchName = "feature/integration-test",
                CommitMessage = "Add test file",
                FilePath = $"test-file-{UtcStamp()}.txt",
                Content = "This is a test file created by integration test.",
            };

            AzureDevOpsActionResult<string> commitIdResult = await _reposClient.CommitAddFileAsync(fileCommitOptions);
            string commitId = commitIdResult.Value!;

            AzureDevOpsActionResult<int> prIdResult = await _reposClient.CreatePullRequestAsync(createOptions);
            int prId = prIdResult.Value;
            _createdPrIds.Add(prId);

            AzureDevOpsActionResult<(string localId, string displayName)> reviewerResult = await _identityClient.GetUserLocalIdFromEmailAsync(_userEmail);

            AzureDevOpsActionResult<bool> addReviewersResult = await _reposClient.AddReviewersAsync(_repoName, prId, [reviewerResult.Value]);
            Assert.True(addReviewersResult.IsSuccessful);

            AzureDevOpsActionResult<GitPullRequest> withReviewerResult = await _reposClient.GetPullRequestAsync(_repoName, prId);
            GitPullRequest? withReviewer = withReviewerResult.Value;
            Assert.Contains(withReviewer!.Reviewers, r => r.Id == reviewerResult.Value.localId);

            AzureDevOpsActionResult<bool> removeReviewersResult = await _reposClient.RemoveReviewersAsync(_repoName, prId, reviewerResult.Value.localId);
            Assert.True(removeReviewersResult.IsSuccessful);

            AzureDevOpsActionResult<GitPullRequest> afterRemoveResult = await _reposClient.GetPullRequestAsync(_repoName, prId);
            GitPullRequest? afterRemove = afterRemoveResult.Value;
            Assert.DoesNotContain(afterRemove!.Reviewers, r => r.Id == reviewerResult.Value.localId);

            AzureDevOpsActionResult<bool> addReviewersAgainResult = await _reposClient.AddReviewersAsync(_repoName, prId, [reviewerResult.Value]);
            Assert.True(addReviewersAgainResult.IsSuccessful);

            AzureDevOpsActionResult<IdentityRefWithVote> identityRefWithVoteResult = await _reposClient.SetReviewerVoteAsync(_repoName, prId, reviewerResult.Value.localId, 10);
            IdentityRefWithVote identityRefWithVote = identityRefWithVoteResult.Value!;

            AzureDevOpsActionResult<GitPullRequestStatus> gitPullRequestStatusResult = await _reposClient.SetPullRequestStatusAsync(_repoName, prId, new PullRequestStatusOptions
            {
                ContextName = "ci/test",
                ContextGenre = "build",
                Description = "Integration status",
                State = GitStatusState.Succeeded,
                TargetUrl = "https://example.com"
            });
            GitPullRequestStatus gitPullRequestStatus = gitPullRequestStatusResult.Value!;

            AzureDevOpsActionResult<(string localId, string displayName)> userResult = await _identityClient.GetUserLocalIdFromEmailAsync(_userEmail);

            AzureDevOpsActionResult<GitPullRequest> gitPullRequestResult = await _reposClient.EnableAutoCompleteAsync(
                _repoName,
                prId,
                userResult.Value.displayName,
                userResult.Value.localId,
                new GitPullRequestCompletionOptions { MergeStrategy = GitPullRequestMergeStrategy.Squash });
            GitPullRequest gitPullRequest = gitPullRequestResult.Value!;

            AzureDevOpsActionResult<GitPullRequest>? afterAutoResult = null;
            await WaitHelper.WaitUntilAsync(async () =>
            {
                afterAutoResult = await _reposClient.GetPullRequestAsync(_repoName, prId);
                return afterAutoResult.IsSuccessful;
            }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

            Assert.NotNull(afterAutoResult!.Value);

            GitPullRequest? afterAuto = afterAutoResult.Value;
            Assert.Equal(afterAuto?.AutoCompleteSetBy.DisplayName, userResult.Value.displayName);
            */
        }

        [Fact]
        public async Task RepositoryCreateDelete_SucceedsAsync()
        {
            string repoName = $"it-repo-{UtcStamp()}";
            AzureDevOpsActionResult<Guid> newRepoIdResult = await _reposClient.CreateRepositoryAsync(repoName);
            Guid newRepoId = newRepoIdResult.Value;
            Assert.True(newRepoIdResult.IsSuccessful);

            AzureDevOpsActionResult<GitRepository> createdResult = await _reposClient.GetRepositoryAsync(newRepoId);
            Assert.True(createdResult.IsSuccessful);
            GitRepository? created = createdResult.Value;
            Assert.NotNull(created);

            AzureDevOpsActionResult<bool> deleteRepoResult = await _reposClient.DeleteRepositoryAsync(newRepoId);
            Assert.True(deleteRepoResult.IsSuccessful);

            AzureDevOpsActionResult<GitRepository> deletedResult = await _reposClient.GetRepositoryAsync(newRepoId);
            GitRepository? deleted = deletedResult.Value;
            Assert.Null(deleted);
        }

        [Fact]
        //public async Task UpdateIterationsAndThreads_SucceedsAsync()
        //{
        //    var createOptions = new PullRequestCreateOptions
        //    {
        //        ProjectIdOrName = _azureDevOpsConfiguration.ProjectName,
        //        RepositoryIdOrName = _repoName,
        //        Title = $"Update PR {UtcStamp()}",
        //        Description = "PR to exercise update and iteration APIs",
        //        SourceBranch = _srcBranch,
        //        TargetBranch = _targetBranch,
        //        IsDraft = false
        //    };

        //    AzureDevOpsActionResult<GitRef> gitRefResult = await _reposClient.GetBranchAsync(_repoName, createOptions.SourceBranch);
        //    GitRef? gitRef = gitRefResult.Value;

        //    if(string.IsNullOrEmpty(gitRef?.Name))
        //    {
        //        AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommitsResult = await _reposClient.GetLatestCommitsAsync(
        //            _azureDevOpsConfiguration.ProjectName,
        //            _repoName,
        //            "main",
        //            top: 1);

        //        IReadOnlyList<GitCommitRef> latestCommits = latestCommitsResult.Value ?? [];
        //        if(latestCommits.Count == 0)
        //            return;

        //        string commitSha = latestCommits[0].CommitId;
        //        AzureDevOpsActionResult<List<GitRefUpdateResult>> branchResult = await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
        //        Assert.True(branchResult.IsSuccessful);
        //    }

        //    AzureDevOpsActionResult<int> prIdResult = await _reposClient.CreatePullRequestAsync(createOptions);
        //    int prId = prIdResult.Value;
        //    _createdPrIds.Add(prId);

        //    var updateOptions = new PullRequestUpdateOptions
        //    {
        //        Title = "Updated by integration test",
        //        Description = "Updated description",
        //        IsDraft = false
        //    };
        //    AzureDevOpsActionResult<GitPullRequest> updateResult = await _reposClient.UpdatePullRequestAsync(_repoName, prId, updateOptions);
        //    Assert.True(updateResult.IsSuccessful);

        //    AzureDevOpsActionResult<IReadOnlyList<GitPullRequestIteration>> iterationsResult = await _reposClient.ListIterationsAsync(_repoName, prId);
        //    IReadOnlyList<GitPullRequestIteration> iterations = iterationsResult.Value ?? [];
        //    Assert.NotEmpty(iterations);

        //    if(iterations[0]?.Id == null)
        //    {
        //        throw new InvalidOperationException("Iteration ID is null, cannot proceed with changes retrieval.");
        //    }

        //    AzureDevOpsActionResult<GitPullRequestIterationChanges> changesResult = await _reposClient.GetIterationChangesAsync(_repoName, prId, iterations[0].Id!.Value);
        //    GitPullRequestIterationChanges changes = changesResult.Value!;
        //    Assert.NotNull(changes);

        //    AzureDevOpsActionResult<int> threadIdResult = await _reposClient.CreateCommentThreadAsync(new CommentThreadOptions
        //    {
        //        RepositoryId = _repoName,
        //        PullRequestId = prId,
        //        Comment = "Initial comment"
        //    });
        //    int threadId = threadIdResult.Value;

        //    AzureDevOpsActionResult<IReadOnlyList<GitPullRequestCommentThread>> threadsResult = await _reposClient.ListPullRequestThreadsAsync(_repoName, prId);
        //    IReadOnlyList<GitPullRequestCommentThread> threads = threadsResult.Value ?? [];
        //    Assert.Contains(threads, t => t.Id == threadId);

        //    AzureDevOpsActionResult<IReadOnlyList<Comment>> commentsResult = await _reposClient.ListPullRequestThreadCommentsAsync(_repoName, prId, threadId);
        //    IReadOnlyList<Comment> comments = commentsResult.Value ?? [];
        //    int commentId = comments.First().Id;

        //    AzureDevOpsActionResult<Comment> editCommentResult = await _reposClient.EditCommentAsync(new CommentEditOptions
        //    {
        //        Repository = _repoName,
        //        PullRequest = prId,
        //        ThreadId = threadId,
        //        CommentId = commentId,
        //        NewContent = "Updated"
        //    });
        //    Assert.True(editCommentResult.IsSuccessful);

        //    AzureDevOpsActionResult<GitPullRequestCommentThread> resolveThreadResult = await _reposClient.ResolveCommentThreadAsync(_repoName, prId, threadId);
        //    Assert.True(resolveThreadResult.IsSuccessful);

        //    AzureDevOpsActionResult<bool> deleteCommentResult = await _reposClient.DeleteCommentAsync(_repoName, prId, threadId, commentId);
        //    Assert.True(deleteCommentResult.IsSuccessful);
        //}

        //[Fact]
        //public async Task RepositoryBranchAndSearchWorkflow_SucceedsAsync()
        //{
        //    AzureDevOpsActionResult<IReadOnlyList<GitRepository>> repositoriesResult = await _reposClient.ListRepositoriesAsync();
        //    IReadOnlyList<GitRepository> repositories = repositoriesResult.Value ?? [];
        //    Assert.NotEmpty(repositories);

        //    AzureDevOpsActionResult<GitRepository> repoByNameResult = await _reposClient.GetRepositoryByNameAsync(_repoName);
        //    GitRepository? repoByName = repoByNameResult.Value;
        //    Assert.NotNull(repoByName);

        //    AzureDevOpsActionResult<IReadOnlyList<GitPullRequest>> projectPullRequestsResult = await _reposClient.ListPullRequestsByProjectAsync(new PullRequestSearchOptions
        //    {
        //        Status = PullRequestStatus.Active
        //    });
        //    IReadOnlyList<GitPullRequest> projectPullRequests = projectPullRequestsResult.Value ?? [];
        //    Assert.NotNull(projectPullRequests);

        //    string branchName = _srcBranch.Replace("refs/heads/", string.Empty);
        //    AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> latestCommitsResult = await _reposClient.GetLatestCommitsAsync(
        //        _azureDevOpsConfiguration.ProjectName,
        //        _repoName,
        //        branchName,
        //        top: 1);

        //    IReadOnlyList<GitCommitRef> latestCommits = latestCommitsResult.Value ?? [];
        //    if(latestCommits.Count == 0)
        //        return;

        //    string commitSha = latestCommits[0].CommitId;
        //    string newBranchRef = $"refs/heads/it-{UtcStamp()}";
        //    AzureDevOpsActionResult<List<GitRefUpdateResult>> branchResult = await _reposClient.CreateBranchAsync(_repoName, newBranchRef, commitSha);
        //    Assert.True(branchResult.IsSuccessful);

        //    AzureDevOpsActionResult<GitRef> branchResult2 = await _reposClient.GetBranchAsync(_repoName, branchName);
        //    GitRef? branch = branchResult2.Value;
        //    Assert.NotNull(branch);

        //    AzureDevOpsActionResult<IReadOnlyList<GitRef>>? myBranchesResult = null;

        //    await WaitHelper.WaitUntilAsync(async () =>
        //    {
        //        myBranchesResult = await _reposClient.ListMyBranchesAsync(_repoName);
        //        return myBranchesResult.IsSuccessful && myBranchesResult.Value?.Count > 0;
        //    }, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        //    IReadOnlyList<GitRef> myBranches = myBranchesResult?.Value ?? [];
        //    Assert.NotEmpty(myBranches);

        //    var searchCriteria = new GitQueryCommitsCriteria
        //    {
        //        FromDate = DateTime.UtcNow.AddMonths(-1).ToString("o"),
        //        ItemVersion = new GitVersionDescriptor
        //        {
        //            Version = branchName,
        //            VersionType = GitVersionType.Branch
        //        }
        //    };
        //    AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>> foundCommitsResult = await _reposClient.SearchCommitsAsync(_repoName, searchCriteria, top: 1);
        //    IReadOnlyList<GitCommitRef> foundCommits = foundCommitsResult.Value ?? [];

        //    Assert.NotNull(foundCommits[0]?.CommitId);
        //    Assert.NotEmpty(foundCommits[0].CommitId);
        //}

        //public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            for(int i = _createdPrIds.Count - 1; i >= 0; i--)
            {
                int id = _createdPrIds[i];
                AzureDevOpsActionResult<GitPullRequest> prResult = await _reposClient.GetPullRequestAsync(_repoName, id);
                GitPullRequest? pr = prResult.Value;
                if(pr != null && pr.Status != PullRequestStatus.Completed)
                {
                    AzureDevOpsActionResult<GitPullRequest> abandonResult = await _reposClient.AbandonPullRequestAsync(_repoName, id);
                    Assert.True(abandonResult.IsSuccessful);
                }
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}