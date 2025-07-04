using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Repos.IntegrationTests
{
    [ExcludeFromCodeCoverage]
    public class DotnetAzureDevOpsReposIntegrationTests : IAsyncLifetime
    {
        private readonly ReposClient _reposClient;

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
            _azureDevOpsConfiguration = new AzureDevOpsConfiguration();
            _repoName = _azureDevOpsConfiguration.RepoName ?? string.Empty;
            _srcBranch = _azureDevOpsConfiguration.SrcBranch;
            _targetBranch = _azureDevOpsConfiguration.TargetBranch;
            _userEmail = _azureDevOpsConfiguration.BotUserEmail;

            _reposClient = new ReposClient(
                _azureDevOpsConfiguration.OrganizationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }


        [Fact]
        public async Task CreateReadCompletePullRequest_SucceedsAsync()
        {
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

            GitPullRequest? completed = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId.Value);
            Assert.Equal(PullRequestStatus.Completed, completed!.Status);
        }


        [Fact(Skip = "API is in preview, // PREVIEW  https://learn.microsoft.com/en-us/dotnet/api/microsoft." +
            "teamfoundation.sourcecontrol.webapi.githttpclientbase.createpullrequestreviewersasync?view=azure-devops-dotnet")]
        public async Task ListAndReviewers_Workflow_SucceedsAsync()
        {
            var opts = new PullRequestCreateOptions
            {
                RepositoryIdOrName = _repoName,
                Title = $"IT PR {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = "PR created by integration test",
                SourceBranch = _srcBranch,
                TargetBranch = _targetBranch
            };

            int pullRequestId = (await _reposClient.CreatePullRequestAsync(opts)).Value;
            _createdPrIds.Add(pullRequestId);

            if(!string.IsNullOrWhiteSpace(_userEmail))
            {
                (string guid, string name) reviewer = await ReviewersClient.GetUserIdFromEmailAsync(
                    _azureDevOpsConfiguration.OrganizationUrl,
                    _azureDevOpsConfiguration.PersonalAccessToken,
                    _userEmail);

                if(string.IsNullOrWhiteSpace(reviewer.guid) && string.IsNullOrWhiteSpace(reviewer.name))
                {
                    throw new InvalidOperationException($"Could not find user with email {_userEmail} in Azure DevOps.");
                }

                (string guid, string name)[] reviewers = [reviewer];

                await _reposClient.AddReviewersAsync(_repoName, pullRequestId, reviewers);

                GitPullRequest? prAfterReviewer = await _reposClient.GetPullRequestAsync(_repoName, pullRequestId);
                Assert.Contains(prAfterReviewer!.Reviewers, r => r.Id == reviewer.guid);

                await _reposClient.SetReviewerVoteAsync(_repoName, pullRequestId, reviewer.guid, 10);
            }

            IReadOnlyList<GitPullRequest> list = await _reposClient.ListPullRequestsAsync(
                _repoName,
                new PullRequestSearchOptions { Status = PullRequestStatus.Active });

            Assert.Contains(list, p => p.PullRequestId == pullRequestId);
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

            await _reposClient.AddLabelsAsync(_repoName, pullRequestId, "docs", "ready");

            IReadOnlyList<WebApiTagDefinition> webApiTagDefinitions = await _reposClient.GetPullRequestLabelsAsync(_repoName, pullRequestId);
            Assert.Contains("docs", webApiTagDefinitions!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);
            Assert.Contains("ready", webApiTagDefinitions!.Select(l => l.Name), StringComparer.OrdinalIgnoreCase);

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
        }



        [Fact]
        public async Task Tags_CreateListDelete_Workflow_SucceedsAsync()
        {

            string? latestCommitSha = (await _reposClient.GetLatestCommitsAsync(
                _azureDevOpsConfiguration.ProjectName,
                _repoName,
                _azureDevOpsConfiguration.MainBranchName))
                .FirstOrDefault()?.CommitId;

            if(string.IsNullOrWhiteSpace(latestCommitSha))
                return;               // skip test if no commit SHA provided (keeps CI green)

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

            GitAnnotatedTag tag =
                await _reposClient.GetTagAsync(_repoName, gitAnnotatedTag.ObjectId);

            Assert.True(tag.Name == annTag);

            GitRefUpdateResult? result = await _reposClient.DeleteTagAsync(_repoName, annTag);


            Assert.NotNull(result);
            Assert.True(result.Success);

        }


        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (int id in _createdPrIds.AsEnumerable().Reverse()) 
            {
                GitPullRequest? pr = await _reposClient.GetPullRequestAsync(_repoName, id);
                if (pr != null && pr.Status != PullRequestStatus.Completed)
                    await _reposClient.AbandonPullRequestAsync(_repoName, id);
            }
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}
