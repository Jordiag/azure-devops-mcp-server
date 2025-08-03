using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;

namespace Dotnet.AzureDevOps.Core.ProjectSettings.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.ProjectSettings)]
    public class DotnetAzureDevOpsProjectSettingsIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly ProjectSettingsClient _projectSettingsClient;
        private readonly WorkItemsClient _workItemsClient;

        public DotnetAzureDevOpsProjectSettingsIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();
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
            Assert.NotEmpty(boardReferenceList);
            List<TeamSettingsIteration> iterations = await _workItemsClient.GetTeamIterationsAsync(teamContext, "");
            Assert.NotEmpty(iterations);

            IReadOnlyList<BoardColumn> cols = await _workItemsClient.ListBoardColumnsAsync(teamContext, boardReferenceList[0].Id, testTeamName);
            await _projectSettingsClient.DeleteTeamAsync(await _projectSettingsClient.GetTeamIdAsync(testTeamName));

            Assert.NotEmpty(cols);

            IReadOnlyList<TeamSettingsIteration> iterationList = await _workItemsClient.ListIterationsAsync(teamContext, "current", _azureDevOpsConfiguration.ProjectName);
            Assert.NotEmpty(iterationList);

            TeamFieldValues areas = await _workItemsClient.ListAreasAsync(teamContext);
            Assert.NotEmpty(areas.Values);
        }

        [Fact]
        public async Task CreateUpdateAndDeleteTeam_SucceedsAsync()
        {
            string teamName = $"it-team-{UtcStamp()}";

            bool created = await _projectSettingsClient.CreateTeamAsync(teamName, "initial");
            Assert.True(created);

            Guid id = await _projectSettingsClient.GetTeamIdAsync(teamName);
            Assert.NotEqual(Guid.Empty, id);

            bool updated = await _projectSettingsClient.UpdateTeamDescriptionAsync(teamName, "updated");
            Assert.True(updated);

            bool deleted = await _projectSettingsClient.DeleteTeamAsync(id);
            Assert.True(deleted);
        }

        [Fact]
        public async Task CreateAndDeleteInheritedProcess_ExecutesAsync()
        {
            string processName = $"it-proc-{UtcStamp()}";

            bool created = await _projectSettingsClient.CreateInheritedProcessAsync(processName, "desc", "Agile");
            Assert.True(created);

            await Task.Delay(2000);

            bool deleted = await _projectSettingsClient.DeleteInheritedProcessAsync("00000000-0000-0000-0000-000000000000");
            Assert.False(deleted);
        }

        [Fact]
        public async Task GetProjectAsync_ReturnsProject_WhenProjectExistsAsync()
        {
            string projectName = _azureDevOpsConfiguration.ProjectName;

            TeamProject? retrievedProject = await _projectSettingsClient.GetProjectAsync(projectName);

            Assert.NotNull(retrievedProject);
            Assert.Equal(projectName, retrievedProject!.Name);
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
