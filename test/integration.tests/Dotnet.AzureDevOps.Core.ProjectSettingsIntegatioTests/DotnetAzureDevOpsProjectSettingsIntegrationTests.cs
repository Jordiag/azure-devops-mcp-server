using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;

namespace Dotnet.AzureDevOps.Core.ProjectSettingsIntegatioTests
{
    [ExcludeFromCodeCoverage]
    public class DotnetAzureDevOpsProjectSettingsIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly ProjectSettingsClient _projectSettingsClient;
        private readonly WorkItemsClient _workItemsClient;

        public DotnetAzureDevOpsProjectSettingsIntegrationTests()
        {
            _azureDevOpsConfiguration = new AzureDevOpsConfiguration();
            _projectSettingsClient = new ProjectSettingsClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
            _workItemsClient = new WorkItemsClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        [Fact]
        public async Task TeamAndBoardConfiguration_SucceedsAsync()
        {
            string testTeamName = "Dotnet.McpIntegrationTest Team";
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName, testTeamName);
            string boardName = $"{_azureDevOpsConfiguration.ProjectName} Team";
            await _projectSettingsClient.CreateTeamAsync(testTeamName, "description1");
            await _projectSettingsClient.UpdateTeamDescriptionAsync(testTeamName, "description2");
            List<BoardReference> boardReferenceList = await _workItemsClient.ListBoardsAsync(teamContext, boardName);
            List<TeamSettingsIteration> iterations = await _workItemsClient.GetTeamIterationsAsync(teamContext, "");

            IReadOnlyList<BoardColumn> cols = await _workItemsClient.ListBoardColumnsAsync(teamContext, boardReferenceList[0].Id, testTeamName);
            await _projectSettingsClient.DeleteTeamAsync(await _projectSettingsClient.GetTeamIdAsync(testTeamName));

            Assert.NotNull(cols);

            IReadOnlyList<TeamSettingsIteration> iterationList = await _workItemsClient.ListIterationsAsync(teamContext, "current", _azureDevOpsConfiguration.ProjectName);
            Assert.NotNull(iterations);
            Assert.NotNull(iterationList);

            TeamFieldValues areas = await _workItemsClient.ListAreasAsync(teamContext);
            Assert.NotNull(areas);
        }

        [Fact]
        public async Task TeamLifecycle_SucceedsAsync()
        {
            string name = $"McpIntTeam-{Guid.NewGuid():N}";

            bool created = await _projectSettingsClient.CreateTeamAsync(name, "desc1");
            Assert.True(created);

            Guid id = await _projectSettingsClient.GetTeamIdAsync(name);
            Assert.NotEqual(Guid.Empty, id);

            bool updated = await _projectSettingsClient.UpdateTeamDescriptionAsync(name, "desc2");
            Assert.True(updated);

            bool deleted = await _projectSettingsClient.DeleteTeamAsync(id);
            Assert.True(deleted);
        }

        [Fact]
        public async Task InheritedProcessLifecycle_SucceedsAsync()
        {
            string processName = $"McpIntProcess-{Guid.NewGuid():N}";

            string? processId = await _projectSettingsClient.CreateInheritedProcessAsync(processName, "desc", "Agile");
            Assert.False(string.IsNullOrEmpty(processId));

            if(processId != null)
            {
                bool deleted = await _projectSettingsClient.DeleteInheritedProcessAsync(processId);
                Assert.True(deleted);
            }
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
