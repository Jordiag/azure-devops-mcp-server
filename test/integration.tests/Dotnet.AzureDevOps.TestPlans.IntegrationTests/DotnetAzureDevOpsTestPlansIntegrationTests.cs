using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Dotnet.AzureDevOps.Core.Pipelines;
using Dotnet.AzureDevOps.Core.Pipelines.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.TestResults.WebApi;

namespace Dotnet.AzureDevOps.TestPlans.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.TestPlans)]
    public class DotnetAzureDevOpsTestPlansIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly TestPlansClient _testPlansClient;
        private readonly PipelinesClient _pipelinesClient;
        private readonly List<int> _createdPlanIds = new List<int>();
        private readonly List<int> _queuedBuildIds = new List<int>();

        public DotnetAzureDevOpsTestPlansIntegrationTests(IntegrationTestFixture fixture)
        {
            _azureDevOpsConfiguration = fixture.Configuration;

            _testPlansClient = fixture.TestPlansClient;
            _pipelinesClient = fixture.PipelinesClient;
        }

        [Fact]
        public async Task PlanCrud_SucceedsAsync()
        {
            TestPlanCreateOptions create = new TestPlanCreateOptions
            {
                Name = $"it-plan-{UtcStamp()}",
                Description = "Created by integration test",
            };

            AzureDevOpsActionResult<int> createResult = await _testPlansClient.CreateTestPlanAsync(create);
            Assert.True(createResult.IsSuccessful);
            int planId = createResult.Value;
            _createdPlanIds.Add(planId);

            AzureDevOpsActionResult<TestPlan?> readResult = await _testPlansClient.GetTestPlanAsync(planId);
            Assert.True(readResult.IsSuccessful);
            TestPlan? read = readResult.Value;
            Assert.NotNull(read);
            Assert.Equal(create.Name, read!.Name);

            AzureDevOpsActionResult<IReadOnlyList<TestPlan>> listResult = await _testPlansClient.ListTestPlansAsync();
            Assert.True(listResult.IsSuccessful);
            IReadOnlyList<TestPlan> list = listResult.Value!;
            Assert.Contains(list, p => p.Id == planId);

            AzureDevOpsActionResult<bool> deleteResult = await _testPlansClient.DeleteTestPlanAsync(planId);
            Assert.True(deleteResult.IsSuccessful);
            _createdPlanIds.Remove(planId);

            AzureDevOpsActionResult<TestPlan?> afterDeleteResult = await _testPlansClient.GetTestPlanAsync(planId);
            Assert.True(afterDeleteResult.IsSuccessful);
            Assert.Null(afterDeleteResult.Value);
        }

        [Fact]
        public async Task SuiteCrud_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> planResult = await _testPlansClient.CreateTestPlanAsync(new TestPlanCreateOptions
            {
                Name = $"it-plan-{UtcStamp()}"
            });
            Assert.True(planResult.IsSuccessful);
            int planId = planResult.Value;
            _createdPlanIds.Add(planId);

            AzureDevOpsActionResult<TestSuite> rootSuiteResult = await _testPlansClient.GetRootSuiteAsync(planId);
            Assert.True(rootSuiteResult.IsSuccessful);
            TestSuite rootSuite = rootSuiteResult.Value!;

            TestSuiteCreateOptions suiteCreate = new TestSuiteCreateOptions
            {
                Name = $"it-suite-{UtcStamp()}",
                ParentSuite = new TestSuiteReference
                {
                    Id = rootSuite.Id
                }
            };

            AzureDevOpsActionResult<int> suiteResult = await _testPlansClient.CreateTestSuiteAsync(planId, suiteCreate);
            Assert.True(suiteResult.IsSuccessful);
            int suiteId = suiteResult.Value;

            AzureDevOpsActionResult<IReadOnlyList<TestSuite>> listSuitesResult = await _testPlansClient.ListTestSuitesAsync(planId);
            Assert.True(listSuitesResult.IsSuccessful);
            IReadOnlyList<TestSuite> suites = listSuitesResult.Value!;
            Assert.Contains(suites, s => s.Id == suiteId);
        }

        [Fact]
        public async Task TestCaseCreateAndAdd_SucceedsAsync()
        {
            AzureDevOpsActionResult<int> planResult = await _testPlansClient.CreateTestPlanAsync(new TestPlanCreateOptions
            {
                Name = $"it-plan-{UtcStamp()}"
            });
            Assert.True(planResult.IsSuccessful);
            int planId = planResult.Value;
            _createdPlanIds.Add(planId);

            AzureDevOpsActionResult<TestSuite> rootResult = await _testPlansClient.GetRootSuiteAsync(planId);
            Assert.True(rootResult.IsSuccessful);
            TestSuite rootSuite = rootResult.Value!;

            AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem?> testCaseResult =
                await _testPlansClient.CreateTestCaseAsync(
                    new TestCaseCreateOptions
                    {
                        Title = "Integration Test Case",
                        Project = _azureDevOpsConfiguration.ProjectName
                    });
            Assert.True(testCaseResult.IsSuccessful);
            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem? testCase = testCaseResult.Value;
            Assert.NotNull(testCase);

            AzureDevOpsActionResult<bool> addResult =
                await _testPlansClient.AddTestCasesAsync(planId, rootSuite.Id, new List<int> { testCase!.Id!.Value });
            Assert.True(addResult.IsSuccessful);

            AzureDevOpsActionResult<Microsoft.VisualStudio.Services.WebApi.PagedList<TestCase>> listResult =
                await _testPlansClient.ListTestCasesAsync(planId, rootSuite.Id);
            Assert.True(listResult.IsSuccessful);
            Microsoft.VisualStudio.Services.WebApi.PagedList<TestCase> list = listResult.Value!;
            Assert.Contains(list, w => w.workItem.Id == testCase.Id);
        }

        [Fact]
        public async Task TestResultsForQueuedBuild_ReturnsDetailsAsync()
        {
            int buildId = await _pipelinesClient.QueueRunAsync(new BuildQueueOptions
            {
                DefinitionId = _azureDevOpsConfiguration.PipelineDefinitionId,
                Branch = _azureDevOpsConfiguration.BuildBranch
            });
            _queuedBuildIds.Add(buildId);

            AzureDevOpsActionResult<TestResultsDetails> detailsResult = await _testPlansClient.GetTestResultsForBuildAsync(
                _azureDevOpsConfiguration.ProjectName,
                buildId);

            Assert.True(detailsResult.IsSuccessful);
            Assert.NotNull(detailsResult.Value);
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach(int id in _createdPlanIds.AsEnumerable().Reverse())
            {
                try
                {
                    _ = await _testPlansClient.DeleteTestPlanAsync(id);
                }
                catch
                {
                    // Ignore errors during cleanup; the test plan may have already been deleted or not exist
                }
            }

            foreach(int buildId in _queuedBuildIds.AsEnumerable().Reverse())
            {
                try
                {
                    Build? build = await _pipelinesClient.GetRunAsync(buildId);
                    if(build != null && build.Status == BuildStatus.InProgress)
                        await _pipelinesClient.CancelRunAsync(buildId, build.Project);
                }
                catch
                {
                    // Ignore failures when cancelling queued builds
                }
            }
        }
    }
}

