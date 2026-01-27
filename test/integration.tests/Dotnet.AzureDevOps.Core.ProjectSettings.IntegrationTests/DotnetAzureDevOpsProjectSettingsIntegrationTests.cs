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
            var teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName, testTeamName);
            string boardName = $"{_azureDevOpsConfiguration.ProjectName} Team";
            await _projectSettingsClient.CreateTeamIfDoesNotExistAsync(testTeamName, "description1");
            await _projectSettingsClient.UpdateTeamDescriptionAsync(testTeamName, "description2");
            AzureDevOpsActionResult<IReadOnlyList<BoardReference>> boardReferenceResult = await _workItemsClient.ListBoardsAsync(teamContext, boardName);
            Assert.True(boardReferenceResult.IsSuccessful);
            IReadOnlyList<BoardReference> boardReferenceList = boardReferenceResult.Value;
            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> iterationsResult = await _workItemsClient.GetTeamIterationsAsync(teamContext, string.Empty);
            Assert.True(iterationsResult.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> iterations = iterationsResult.Value;

            AzureDevOpsActionResult<IReadOnlyList<BoardColumn>> colsResult = await _workItemsClient.ListBoardColumnsAsync(teamContext, boardReferenceList[0].Id, testTeamName);
            Assert.True(colsResult.IsSuccessful);
            IReadOnlyList<BoardColumn> cols = colsResult.Value;
            AzureDevOpsActionResult<Guid> teamIdResult = await _projectSettingsClient.GetTeamIdAsync(testTeamName);
            Guid teamId = teamIdResult.Value;
            await _projectSettingsClient.DeleteTeamAsync(teamId);

            Assert.NotEmpty(cols);

            AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>> iterationListResult = await _workItemsClient.ListIterationsAsync(teamContext, "current", _azureDevOpsConfiguration.ProjectName);
            Assert.True(iterationListResult.IsSuccessful);
            IReadOnlyList<TeamSettingsIteration> iterationList = iterationListResult.Value;

            AzureDevOpsActionResult<TeamFieldValues> areasResult = await _workItemsClient.ListAreasAsync(teamContext);
            Assert.True(areasResult.IsSuccessful);
            TeamFieldValues areas = areasResult.Value;
            Assert.NotEmpty(areas.Values);
        }

        [Fact]
        public async Task CreateUpdateAndDeleteTeam_SucceedsAsync()
        {
            string teamName = $"it-team-{UtcStamp()}";

            AzureDevOpsActionResult<bool> createdResult = await _projectSettingsClient.CreateTeamIfDoesNotExistAsync(teamName, "initial");
            Assert.True(createdResult.IsSuccessful && createdResult.Value);

            AzureDevOpsActionResult<Guid> idResult = await _projectSettingsClient.GetTeamIdAsync(teamName);
            Guid id = idResult.Value;
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

        public ValueTask InitializeAsync() => ValueTask.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
