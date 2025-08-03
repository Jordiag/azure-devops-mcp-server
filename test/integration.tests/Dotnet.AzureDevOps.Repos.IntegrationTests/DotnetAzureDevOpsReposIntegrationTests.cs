using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Repos.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Repos)]
    public class DotnetAzureDevOpsReposIntegrationTests : IAsyncLifetime
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

        public DotnetAzureDevOpsReposIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();
            _repoName = _azureDevOpsConfiguration.RepoName ?? string.Empty;
            _srcBranch = _azureDevOpsConfiguration.SrcBranch;
            _targetBranch = _azureDevOpsConfiguration.TargetBranch;
            _userEmail = _azureDevOpsConfiguration.BotUserEmail;

            _reposClient = new ReposClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
            _identityClient = new IdentityClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        public async Task CreateReadCompletePullRequest_SucceedsAsync()
        {
            var createOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"Advanced PR {UtcStamp()}",
                Description = "PR exercising advanced APIs",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            GitRef? gitRef = await _reposClient.GetBranchAsync(_repoName, createOptions.SourceBranch);

            if(string.IsNullOrEmpty(gitRef?.Name))
            {
                IReadOnlyList<GitCommitRef> latestCommits = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repoName,
                    "main",
                    top: 1);

                if(latestCommits.Count == 0)
                    return;

                string commitSha = latestCommits[0].CommitId;
                await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
            }

            var pullRequestCreateOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"Integration PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "Created by automated test",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch,
                IsDraft = false
            };

            int? pullRequestId = await _reposClient.CreatePullRequestAsync(pullRequestCreateOptions);
            Assert.True(pullRequestId.HasValue);
            _createdPrIds.Add(pullRequestId.Value);

            GitPullRequest? gtPullRequest = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId.Value);
            Assert.NotNull(gtPullRequest);
            Assert.Equal(PullRequestStatus.Active, gtPullRequest!.Status);

            await _reposClient.CompletePullRequestAsync(_repoName, pullRequestId.Value, squashMerge: true, gtPullRequest.LastMergeSourceCommit);

            GitPullRequest? completed = await WaitForCompletePullRequestAsync(pullRequestId.Value);

            Assert.Equal(PullRequestStatus.Completed, completed!.Status);
        }

        private async Task<GitPullRequest?> WaitForCompletePullRequestAsync(int pullRequestId)
        {
            const int maxAttempts = 20;
            const int delayMs = 500;

            for(int attempt = 0; attempt < maxAttempts; attempt++)
            {
                GitPullRequest? gtPullRequest = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId);
                if(gtPullRequest?.Status == PullRequestStatus.Completed)
                    return gtPullRequest;
                await Task.Delay(delayMs);
            }
            return default;
        }

        [Fact]
        public async Task ListAndReviewers_Workflow_SucceedsAsync()
        {
            var pullRequestCreateOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"IT PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "PR created by integration test",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            GitRef? gitRef = await _reposClient.GetBranchAsync(_repoName, pullRequestCreateOptions.SourceBranch);

            if(string.IsNullOrEmpty(gitRef?.Name))
            {
                IReadOnlyList<GitCommitRef> latestCommits = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repoName,
                    "main",
                    top: 1);

                if(latestCommits.Count == 0)
                    return;

                string commitSha = latestCommits[0].CommitId;
                await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
            }

            int pullRequestId = (await _reposClient.CreatePullRequestAsync(pullRequestCreateOptions)).Value;
            _createdPrIds.Add(pullRequestId);

            (string localId, string displayName) reviewer = default;
            ;
            short voteValue = 10;

            if(!string.IsNullOrWhiteSpace(_userEmail))
            {
                reviewer = await _identityClient.GetUserLocalIdFromEmailAsync(_userEmail);

                if(string.IsNullOrWhiteSpace(reviewer.localId) || string.IsNullOrWhiteSpace(reviewer.displayName))
                {
                    throw new InvalidOperationException($"Could not find user with email {_userEmail} in Azure DevOps.");
                }

                (string localId, string displayName)[] reviewers = [(reviewer.localId, reviewer.displayName ?? string.Empty)];

                bool success = await _reposClient.AddReviewersAsync(_repoName, pullRequestId, reviewers);
                Assert.True(success, "Failed to add reviewers to the pull request.");

                GitPullRequest? prAfterReviewer = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId);
                Assert.Contains(prAfterReviewer!.Reviewers, r => r.Id == reviewer.localId);

                await _reposClient.SetReviewerVoteAsync(_repoName, pullRequestId, reviewer.localId, voteValue);
            }

            IReadOnlyList<GitPullRequest> list = await _reposClient.ListPullRequestsAsync(
                _repoName,
                new PullRequestSearchOptions { Status = PullRequestStatus.Active });

            Assert.Contains(list, p => p.PullRequestId == pullRequestId);
            Assert.Contains(list, p => p.Reviewers.Any(r => r.DisplayName == reviewer.displayName && r.Vote == voteValue));

        }

        [Fact]
        public async Task LabelsAndCommentsWorkflow_SucceedsAsync()
        {
            // create PR
            var pullRequestCreateOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"Tier2 PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "Tier-2 labels/comments test",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            int pullRequestId = (await _reposClient.CreatePullRequestAsync(pullRequestCreateOptions)).Value;
            _createdPrIds.Add(pullRequestId);

            /* ---------- LABELS ---------- */
            await _reposClient.AddLabelsAsync(_repoName, pullRequestId, "docs", "ready");

            IReadOnlyList<WebApiTagDefinition> webApiTagDefinitions = await _reposClient.GetPullRequestLabelsAsync(_repoName, pullRequestId);
            Assert.Contains("docs", webApiTagDefinitions!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);
            Assert.Contains("ready", webApiTagDefinitions!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);

            IReadOnlyList<GitPullRequest> labelPullRequests = await _reposClient.ListPullRequestsByLabelAsync(
                _repoName,
                "ready",
                PullRequestStatus.Active);
            Assert.Contains(labelPullRequests, pr => pr.PullRequestId == pullRequestId);

            await _reposClient.RemoveLabelAsync(_repoName, pullRequestId, "docs");

            IReadOnlyList<WebApiTagDefinition> webApiTagDefinitions2 = await _reposClient.GetPullRequestLabelsAsync(_repoName, pullRequestId);
            Assert.DoesNotContain("docs", webApiTagDefinitions2!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);

            int commentThreadId = await _reposClient.CreateCommentThreadAsync(new CommentThreadOptions
            {
                RepositoryId = _repoName,
                PullRequestId = pullRequestId,
                Comment = "CI failed, please fix.",
                IsLeftSide = false
            });

            int? commentReplyId = await _reposClient.ReplyToCommentThreadAsync(new CommentReplyOptions
            {
                Repository = _repoName,
                PullRequestId = pullRequestId,
                ThreadId = commentThreadId,
                Comment = "Fixed and repushed!",
                ResolveThread = true
            });

            Assert.NotNull(commentReplyId);

            IReadOnlyList<GitPullRequestCommentThread> threads = await _reposClient.ListPullRequestThreadsAsync(_repoName, pullRequestId);
            Assert.Contains(threads, t => t.Id == commentThreadId);

            IReadOnlyList<Comment> comments = await _reposClient.ListPullRequestThreadCommentsAsync(_repoName, pullRequestId, commentThreadId);
            int commentId = comments.First().Id;

            await _reposClient.EditCommentAsync(new CommentEditOptions
            {
                Repository = _repoName,
                PullRequest = pullRequestId,
                ThreadId = commentThreadId,
                CommentId = commentId,
                NewContent = "Edited"
            });

            await _reposClient.ResolveCommentThreadAsync(_repoName, pullRequestId, commentThreadId);
            await _reposClient.DeleteCommentAsync(_repoName, pullRequestId, commentThreadId, commentId);
        }


        [Fact]
        public async Task Tags_CreateListDelete_Workflow_SucceedsAsync()
        {
            IReadOnlyList<GitCommitRef> latestCommits = await _reposClient.GetLatestCommitsAsync(
                _azureDevOpsConfiguration.ProjectName,
                _repoName,
                _azureDevOpsConfiguration.MainBranchName);

            string? latestCommitSha = latestCommits.Count > 0 ? latestCommits[0].CommitId : null;

            if(string.IsNullOrWhiteSpace(latestCommitSha))
                return;  

            string annTag = $"it-ann-{UtcStamp()}";

            GitAnnotatedTag gitAnnotatedTag = await _reposClient.CreateTagAsync(new TagCreateOptions
            {
                Repository = _repoName,
                Name = annTag,
                CommitSha = latestCommitSha,
                Message = "Integration annotated tag",
                TaggerName = "integration-bot",
                TaggerEmail = "bot@example.com"
            });

            GitAnnotatedTag tag = await _reposClient.GetTagAsync(_repoName, gitAnnotatedTag.ObjectId);

            Assert.True(tag.Name == annTag);

            GitRefUpdateResult? result = await _reposClient.DeleteTagAsync(_repoName, annTag);

            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task BranchListingAndDiff_SucceedsAsync()
        {
            IReadOnlyList<GitRef> branches = await _reposClient.ListBranchesAsync(_repoName);
            Assert.NotEmpty(branches);

            IReadOnlyList<GitCommitRef> commits = await _reposClient.GetLatestCommitsAsync(
                _azureDevOpsConfiguration.ProjectName,
                _repoName,
                _azureDevOpsConfiguration.MainBranchName,
                top: 2);

            if(commits.Count < 2)
                return; // insufficient history

            string baseCommit = commits[1].CommitId;
            string targetCommit = commits[0].CommitId;

            GitCommitDiffs diffs = await _reposClient.GetCommitDiffAsync(_repoName, baseCommit, targetCommit);
            Assert.NotNull(diffs);
        }

        [Fact(Skip = "Vote doesn't work in pipeline")]
        public async Task AdvancedPullRequestWorkflow_SucceedsAsync()
        {
            var createOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"Advanced PR {UtcStamp()}",
                Description = "PR exercising advanced APIs",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            GitRef? gitRef = await _reposClient.GetBranchAsync(_repoName, createOptions.SourceBranch);

            if(string.IsNullOrEmpty(gitRef?.Name))
            {
                IReadOnlyList<GitCommitRef> latestCommits = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repoName,
                    "main",
                    top: 1);

                if(latestCommits.Count == 0)
                    return;

                string commitSha = latestCommits[0].CommitId;
                await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
            }

            FileCommitOptions fileCommitOptions = new FileCommitOptions
            {
                RepositoryName = _azureDevOpsConfiguration.RepoName,
                BranchName = "feature/integration-test",
                CommitMessage = "Add test file",
                FilePath = $"test-file-{UtcStamp()}.txt",
                Content = "This is a test file created by integration test.",
            };

            string commitId = await _reposClient.CommitAddFileAsync(fileCommitOptions);

            int prId = (await _reposClient.CreatePullRequestAsync(createOptions)).Value;
            _createdPrIds.Add(prId);

            (string localId, string displayName) reviewer = await _identityClient.GetUserLocalIdFromEmailAsync(_userEmail);

            await _reposClient.AddReviewersAsync(_repoName, prId, [reviewer]);
            GitPullRequest? withReviewer = await _reposClient.GetPullRequestAsync(_repoName, prId);
            Assert.Contains(withReviewer!.Reviewers, r => r.Id == reviewer.localId);

            await _reposClient.RemoveReviewersAsync(_repoName, prId, reviewer.localId);
            GitPullRequest? afterRemove = await _reposClient.GetPullRequestAsync(_repoName, prId);
            Assert.DoesNotContain(afterRemove!.Reviewers, r => r.Id == reviewer.localId);
            await _reposClient.AddReviewersAsync(_repoName, prId, [reviewer]);

            IdentityRefWithVote identityRefWithVote = await _reposClient.SetReviewerVoteAsync(_repoName, prId, reviewer.localId, 10);
            GitPullRequestStatus gitPullRequestStatus = await _reposClient.SetPullRequestStatusAsync(_repoName, prId, new PullRequestStatusOptions
            {
                ContextName = "ci/test",
                ContextGenre = "build",
                Description = "Integration status",
                State = GitStatusState.Succeeded,
                TargetUrl = "https://example.com"
            });

            (string localId, string displayName) user = await _identityClient.GetUserLocalIdFromEmailAsync(_userEmail);

            GitPullRequest gitPullRequest = await _reposClient.EnableAutoCompleteAsync(
                _repoName,
                prId,
                user.displayName,
                user.localId,
                new GitPullRequestCompletionOptions { MergeStrategy = GitPullRequestMergeStrategy.Squash });
            GitPullRequest? afterAuto = await _reposClient.GetPullRequestAsync(_repoName, prId);
            Assert.Equal(afterAuto?.AutoCompleteSetBy.DisplayName, user.displayName);
        }

        [Fact]
        public async Task RepositoryCreateDelete_SucceedsAsync()
        {
            string repoName = $"it-repo-{UtcStamp()}";
            Guid newRepoId = await _reposClient.CreateRepositoryAsync(repoName);

            GitRepository? created = await _reposClient.GetRepositoryAsync(newRepoId);
            Assert.NotNull(created);

            await _reposClient.DeleteRepositoryAsync(newRepoId);

            GitRepository? deleted = await _reposClient.GetRepositoryAsync(newRepoId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task UpdateIterationsAndThreads_SucceedsAsync()
        {
            var createOptions = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"Update PR {UtcStamp()}",
                Description = "PR to exercise update and iteration APIs",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            GitRef? gitRef = await _reposClient.GetBranchAsync(_repoName, createOptions.SourceBranch);

            if(string.IsNullOrEmpty(gitRef?.Name))
            {
                IReadOnlyList<GitCommitRef> latestCommits = await _reposClient.GetLatestCommitsAsync(
                    _azureDevOpsConfiguration.ProjectName,
                    _repoName,
                    "main",
                    top: 1);

                if(latestCommits.Count == 0)
                    return;

                string commitSha = latestCommits[0].CommitId;
                await _reposClient.CreateBranchAsync(_repoName, _srcBranch, commitSha);
            }

            int prId = (await _reposClient.CreatePullRequestAsync(createOptions)).Value;
            _createdPrIds.Add(prId);

            var updateOptions = new PullRequestUpdateOptions
            {
                Title = "Updated by integration test",
                Description = "Updated description",
                IsDraft = false
            };
            await _reposClient.UpdatePullRequestAsync(_repoName, prId, updateOptions);

            IReadOnlyList<GitPullRequestIteration> iterations = await _reposClient.ListIterationsAsync(_repoName, prId);
            Assert.NotEmpty(iterations);

            if(iterations[0]?.Id == null)
            {
                throw new InvalidOperationException("Iteration ID is null, cannot proceed with changes retrieval.");
            }

            GitPullRequestIterationChanges changes = await _reposClient.GetIterationChangesAsync(_repoName, prId, iterations[0].Id.Value);
            Assert.NotNull(changes);

            int threadId = await _reposClient.CreateCommentThreadAsync(new CommentThreadOptions
            {
                RepositoryId = _repoName,
                PullRequestId = prId,
                Comment = "Initial comment"
            });

            IReadOnlyList<GitPullRequestCommentThread> threads = await _reposClient.ListPullRequestThreadsAsync(_repoName, prId);
            Assert.Contains(threads, t => t.Id == threadId);

            IReadOnlyList<Comment> comments = await _reposClient.ListPullRequestThreadCommentsAsync(_repoName, prId, threadId);
            int commentId = comments.First().Id;

            await _reposClient.EditCommentAsync(new CommentEditOptions
            {
                Repository = _repoName,
                PullRequest = prId,
                ThreadId = threadId,
                CommentId = commentId,
                NewContent = "Updated"
            });

            await _reposClient.ResolveCommentThreadAsync(_repoName, prId, threadId);
            await _reposClient.DeleteCommentAsync(_repoName, prId, threadId, commentId);
        }

        [Fact]
        public async Task RepositoryBranchAndSearchWorkflow_SucceedsAsync()
        {
            IReadOnlyList<GitRepository> repositories = await _reposClient.ListRepositoriesAsync();
            Assert.NotEmpty(repositories);

            GitRepository? repoByName = await _reposClient.GetRepositoryByNameAsync(_repoName);
            Assert.NotNull(repoByName);

            IReadOnlyList<GitPullRequest> projectPullRequests = await _reposClient.ListPullRequestsByProjectAsync(new PullRequestSearchOptions
            {
                Status = PullRequestStatus.Active
            });
            Assert.NotNull(projectPullRequests);

            string branchName = _srcBranch.Replace("refs/heads/", string.Empty);
            IReadOnlyList<GitCommitRef> latestCommits = await _reposClient.GetLatestCommitsAsync(
                _azureDevOpsConfiguration.ProjectName,
                _repoName,
                branchName,
                top: 1);

            if(latestCommits.Count == 0)
                return;

            string commitSha = latestCommits[0].CommitId;
            string newBranchRef = $"refs/heads/it-{UtcStamp()}";
            await _reposClient.CreateBranchAsync(_repoName, newBranchRef, commitSha);

            GitRef? branch = await _reposClient.GetBranchAsync(_repoName, branchName);
            Assert.NotNull(branch);

            IReadOnlyList<GitRef> myBranches = await _reposClient.ListMyBranchesAsync(_repoName);
            Assert.NotEmpty(myBranches);

            var searchCriteria = new GitQueryCommitsCriteria
            {
                FromDate = DateTime.UtcNow.AddMonths(-1).ToString("o"),
                ItemVersion = new GitVersionDescriptor
                {
                    Version = branchName,
                    VersionType = GitVersionType.Branch
                }
            };
            IReadOnlyList<GitCommitRef> foundCommits = await _reposClient.SearchCommitsAsync(_repoName, searchCriteria, top: 1);

            Assert.NotNull(foundCommits[0]?.CommitId);
            Assert.NotEmpty(foundCommits[0].CommitId);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            for(int i = _createdPrIds.Count - 1; i >= 0; i--)
            {
                int id = _createdPrIds[i];
                GitPullRequest? pr = await _reposClient.GetPullRequestAsync(_repoName, id);
                if(pr != null && pr.Status != PullRequestStatus.Completed)
                {
                    await _reposClient.AbandonPullRequestAsync(_repoName, id);
                }
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}
