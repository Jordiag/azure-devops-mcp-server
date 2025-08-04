using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests;

public abstract class BoardsTestBase : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    protected readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
    protected readonly WorkItemsClient _workItemsClient;
    protected readonly ReposClient _reposClient;
    protected readonly ProjectSettingsClient _projectSettingsClient;
    protected readonly List<int> _createdWorkItemIds = new();
    protected readonly List<int> _createdPullRequestIds = new();
    protected readonly List<Guid> _createdProjectIds = new();
    protected readonly string _repositoryName;
    protected readonly string _sourceBranch;
    protected readonly string _targetBranch;
    protected readonly WorkItemTestUtilities _workItemHelper;

    protected BoardsTestBase(IntegrationTestFixture fixture)
    {
        _azureDevOpsConfiguration = fixture.Configuration;
        _workItemsClient = fixture.WorkItemsClient;
        _reposClient = fixture.ReposClient;
        _projectSettingsClient = fixture.ProjectSettingsClient;

        _repositoryName = _azureDevOpsConfiguration.RepoName;
        _sourceBranch = _azureDevOpsConfiguration.SrcBranch;
        _targetBranch = _azureDevOpsConfiguration.TargetBranch;

        _workItemHelper = new WorkItemTestUtilities(_workItemsClient, _createdWorkItemIds);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (int id in _createdWorkItemIds.AsEnumerable().Reverse())
        {
            await _workItemsClient.DeleteWorkItemAsync(id);
        }

        foreach (int prId in _createdPullRequestIds.AsEnumerable().Reverse())
        {
            await _reposClient.AbandonPullRequestAsync(_repositoryName, prId);
        }

        foreach (Guid projectId in _createdProjectIds.AsEnumerable().Reverse())
        {
            await _projectSettingsClient.DeleteProjectAsync(projectId);
        }
    }

    protected static string UtcStamp() =>
        DateTime.UtcNow.ToString("O").Replace(':', '-');
}
