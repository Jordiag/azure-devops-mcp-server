using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Tests.Common;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    public abstract class BoardsIntegrationTestBase : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        protected BoardsIntegrationTestBase(IntegrationTestFixture fixture)
        {
            AzureDevOpsConfiguration = fixture.Configuration;
            WorkItemsClient = fixture.WorkItemsClient;
            ReposClient = fixture.ReposClient;
            ProjectSettingsClient = fixture.ProjectSettingsClient;

            RepositoryName = AzureDevOpsConfiguration.RepoName;
            SourceBranch = AzureDevOpsConfiguration.SrcBranch;
            TargetBranch = AzureDevOpsConfiguration.TargetBranch;
        }

        protected AzureDevOpsConfiguration AzureDevOpsConfiguration { get; }
        protected WorkItemsClient WorkItemsClient { get; }
        protected ReposClient ReposClient { get; }
        protected ProjectSettingsClient ProjectSettingsClient { get; }
        protected List<int> CreatedWorkItemIds { get; } = new List<int>();
        protected List<int> CreatedPullRequestIds { get; } = new List<int>();
        protected List<Guid> CreatedProjectIds { get; } = new List<Guid>();
        protected string RepositoryName { get; }
        protected string SourceBranch { get; }
        protected string TargetBranch { get; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            foreach(int identifier in CreatedWorkItemIds.AsEnumerable().Reverse())
            {
                await WorkItemsClient.DeleteWorkItemAsync(identifier);
            }

            foreach(int identifier in CreatedPullRequestIds.AsEnumerable().Reverse())
            {
                await ReposClient.AbandonPullRequestAsync(RepositoryName, identifier);
            }

            foreach(Guid identifier in CreatedProjectIds.AsEnumerable().Reverse())
            {
                await ProjectSettingsClient.DeleteProjectAsync(identifier);
            }
        }

        protected static string UtcStamp()
        {
            return DateTime.UtcNow.ToString("O").Replace(':', '-');
        }
    }
}
