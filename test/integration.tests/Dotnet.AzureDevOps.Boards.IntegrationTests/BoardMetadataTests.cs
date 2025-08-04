using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Boards)]
    public class BoardMetadataTests : BoardsIntegrationTestBase
    {
        public BoardMetadataTests(IntegrationTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task ListBoards_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<BoardReference> boards = await WorkItemsClient.ListBoardsAsync(teamContext);
            Assert.NotEmpty(boards);
        }

        [Fact]
        public async Task GetTeamIteration_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<TeamSettingsIteration> iterations = await WorkItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(iterations);
            TeamSettingsIteration iteration = iterations.First();
            TeamSettingsIteration retrieved = await WorkItemsClient.GetTeamIterationAsync(teamContext, iteration.Id);
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task GetTeamIterations_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<TeamSettingsIteration> iterations = await WorkItemsClient.GetTeamIterationsAsync(teamContext, "current");
            Assert.NotEmpty(iterations);
        }

        [Fact]
        public async Task ListBoardColumns_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<BoardReference> boards = await WorkItemsClient.ListBoardsAsync(teamContext);
            Assert.NotEmpty(boards);
            Guid boardId = boards.First().Id;
            IList<BoardColumn> columns = await WorkItemsClient.ListBoardColumnsAsync(teamContext, boardId);
            Assert.NotEmpty(columns);
        }

        [Fact]
        public async Task ListBacklogs_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<BacklogLevelConfiguration> backlogs = await WorkItemsClient.ListBacklogsAsync(teamContext);
            Assert.NotEmpty(backlogs);
        }

        [Fact]
        public async Task ListBacklogWorkItems_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            BacklogLevelWorkItems workItems = await WorkItemsClient.ListBacklogWorkItemsAsync(teamContext, "Stories");
            Assert.NotNull(workItems);
        }

        [Fact]
        public async Task ListMyWorkItems_SucceedsAsync()
        {
            PredefinedQuery workItems = await WorkItemsClient.ListMyWorkItemsAsync();
            Assert.NotNull(workItems);
        }

        [Fact]
        public async Task GetWorkItemsForIteration_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<TeamSettingsIteration> iterations = await WorkItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(iterations);
            TeamSettingsIteration iteration = iterations.First();
            IterationWorkItems result = await WorkItemsClient.GetWorkItemsForIterationAsync(teamContext, iteration.Id);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListIterations_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<TeamSettingsIteration> iterations = await WorkItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(iterations);
        }

        [Fact]
        public async Task CreateIterations_SucceedsAsync()
        {
            string name = $"it-{DateTime.UtcNow:yyyyMMddHHmmss}";
            IList<IterationCreateOptions> iterations = new List<IterationCreateOptions>
            {
                new IterationCreateOptions { IterationName = name }
            };

            IReadOnlyList<WorkItemClassificationNode> created = await WorkItemsClient.CreateIterationsAsync(AzureDevOpsConfiguration.ProjectName, iterations);
            Assert.NotEmpty(created);
        }

        [Fact]
        public async Task AssignIterations_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<TeamSettingsIteration> existing = await WorkItemsClient.ListIterationsAsync(teamContext);
            Assert.NotEmpty(existing);
            TeamSettingsIteration iteration = existing.First();
            IList<IterationAssignmentOptions> assignments = new List<IterationAssignmentOptions>
            {
                new IterationAssignmentOptions { Identifier = iteration.Id, Path = iteration.Path! }
            };
            IReadOnlyList<TeamSettingsIteration> result = await WorkItemsClient.AssignIterationsAsync(teamContext, assignments);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ListAreas_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            TeamFieldValues areas = await WorkItemsClient.ListAreasAsync(teamContext);
            Assert.NotEmpty(areas.Values);
        }

        [Fact]
        public async Task CreateCustomFieldIfDoesNotExist_SucceedsAsync()
        {
            WorkItemsClient client = WorkItemsClient;
            string fieldName = $"CustomField{UtcStamp()}".Replace(".", "").Replace("-", "");
            string referenceName = $"Custom.Reference.{UtcStamp()}".Replace(".", "").Replace("-", "");

            if(await WorkItemsClient.IsSystemProcessAsync())
            {
                string processName = $"it-proc-{UtcStamp()}";
                bool processCreated = await ProjectSettingsClient.CreateInheritedProcessAsync(processName, "Custom", "Agile");
                Assert.True(processCreated);
                string? processId = null;
                await WaitHelper.WaitUntilAsync(async () =>
                {
                    processId = await ProjectSettingsClient.GetProcessIdAsync(processName);
                    return !string.IsNullOrEmpty(processId);
                }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
                Assert.False(string.IsNullOrEmpty(processId));

                string projectName = $"it-proj-{UtcStamp()}";
                Guid? projectId = await ProjectSettingsClient.CreateProjectAsync(projectName, "Custom field project", processId!);
                Assert.True(projectId.HasValue);
                CreatedProjectIds.Add(projectId!.Value);

                client = new WorkItemsClient(
                    AzureDevOpsConfiguration.OrganisationUrl,
                    projectName,
                    AzureDevOpsConfiguration.PersonalAccessToken);
            }

            WorkItemField2 first = await client.CreateCustomFieldIfDoesntExistAsync(
                fieldName,
                referenceName,
                FieldType.String,
                "integration test field");
            Assert.Equal(referenceName, first.ReferenceName);

            WorkItemField2 second = await client.CreateCustomFieldIfDoesntExistAsync(
                fieldName,
                referenceName,
                FieldType.String,
                "integration test field");
            Assert.Equal(referenceName, second.ReferenceName);
        }

        [Fact]
        public async Task CustomFieldWorkflow_SucceedsAsync()
        {
            string fieldName = $"CustomField{UtcStamp()}".Replace(".", "").Replace("-", "");
            string referenceName = $"Custom.Reference.{UtcStamp()}".Replace(".", "").Replace("-", "");

            WorkItemField2 field = await WorkItemsClient.CreateCustomFieldAsync(
                fieldName,
                referenceName,
                FieldType.String,
                "integration test field");
            Assert.Equal(referenceName, field.ReferenceName);

            WorkItemField2? retrieved = await WorkItemsClient.GetWorkItemFieldAsync(referenceName);
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task ExportBoard_SucceedsAsync()
        {
            TeamContext teamContext = new TeamContext(AzureDevOpsConfiguration.ProjectName);
            IList<BoardReference> boards = await WorkItemsClient.ListBoardsAsync(teamContext);
            Assert.NotEmpty(boards);
            Board? board = await WorkItemsClient.ExportBoardAsync(teamContext, boards.First().Id.ToString());
            Assert.NotNull(board);
        }
    }
}
