using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;

namespace Dotnet.AzureDevOps.Core.ProjectSettings.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.ProjectSettings)]
    public class DotnetAzureDevOpsProjectSettingsIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly ProjectSettingsClient _projectSettingsClient;
        private readonly WorkItemsClient _workItemsClient;

        public DotnetAzureDevOpsProjectSettingsIntegrationTests(IntegrationTestFixture fixture)
        {
            _azureDevOpsConfiguration = fixture.Configuration;
            _projectSettingsClient = fixture.ProjectSettingsClient;
            _workItemsClient = fixture.WorkItemsClient;
        }

        [Fact]
        public async Task TeamAndBoardConfiguration_SucceedsAsync()
        {
            string testTeamName = "Dotnet.McpIntegrationTest Team";
            TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName, testTeamName);
            string boardName = $"{_azureDevOpsConfiguration.ProjectName} Team";
            await _projectSettingsClient.CreateTeamAsync(testTeamName, "description1");
            await _projectSettingsClient.UpdateTeamDescriptionAsync(testTeamName, "description2");
            List<BoardReference> boardReferenceList = await _workItemsClient.ListBoardsAsync(teamContext, boardName);
            Assert.NotEmpty(boardReferenceList);
            List<TeamSettingsIteration> iterations = await _workItemsClient.GetTeamIterationsAsync(teamContext, "");
            Assert.NotEmpty(iterations);

            IReadOnlyList<BoardColumn> cols = await _workItemsClient.ListBoardColumnsAsync(teamContext, boardReferenceList[0].Id, testTeamName);
            AzureDevOpsActionResult<Guid> teamIdResult = await _projectSettingsClient.GetTeamIdAsync(testTeamName);
            Guid teamId = teamIdResult.Value ?? Guid.Empty;
            await _projectSettingsClient.DeleteTeamAsync(teamId);

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

            AzureDevOpsActionResult<bool> createdResult = await _projectSettingsClient.CreateTeamAsync(teamName, "initial");
            Assert.True(createdResult.IsSuccessful && createdResult.Value);

            AzureDevOpsActionResult<Guid> idResult = await _projectSettingsClient.GetTeamIdAsync(teamName);
            Guid id = idResult.Value ?? Guid.Empty;
            Assert.True(idResult.IsSuccessful);
            Assert.NotEqual(Guid.Empty, id);

            AzureDevOpsActionResult<bool> updatedResult = await _projectSettingsClient.UpdateTeamDescriptionAsync(teamName, "updated");
            Assert.True(updatedResult.IsSuccessful && updatedResult.Value);

            AzureDevOpsActionResult<bool> deletedResult = await _projectSettingsClient.DeleteTeamAsync(id);
            Assert.True(deletedResult.IsSuccessful && deletedResult.Value);
        }

        // TODO: Re-enable this test once the API is working again
        [Fact(Skip = "API not longer working")]
        public async Task CreateAndDeleteInheritedProcess_ExecutesAsync()
        {
            string processName = $"it-proc-{UtcStamp()}";

            AzureDevOpsActionResult<bool> createdResult = await _projectSettingsClient.CreateInheritedProcessAsync(processName, "Inherited Process integration test", "Agile");
            Assert.True(createdResult.IsSuccessful && createdResult.Value, "Process was not created");
            await WaitHelper.WaitUntilAsync(async () =>
            {
                AzureDevOpsActionResult<string> processIdResult = await _projectSettingsClient.GetProcessIdAsync(processName);
                return processIdResult.Value is not null;
            },
                TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

            AzureDevOpsActionResult<bool> deletedResult = await _projectSettingsClient.DeleteInheritedProcessAsync("00000000-0000-0000-0000-000000000000");
            Assert.False(deletedResult.IsSuccessful && deletedResult.Value, "Process was not deleted");
        }

        [Fact]
        public async Task GetProjectAsync_ReturnsProject_WhenProjectExistsAsync()
        {
            string projectName = _azureDevOpsConfiguration.ProjectName;

            AzureDevOpsActionResult<TeamProject> projectResult = await _projectSettingsClient.GetProjectAsync(projectName);
            TeamProject? retrievedProject = projectResult.Value;

            Assert.True(projectResult.IsSuccessful);
            Assert.NotNull(retrievedProject);
            Assert.Equal(projectName, retrievedProject!.Name);
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
