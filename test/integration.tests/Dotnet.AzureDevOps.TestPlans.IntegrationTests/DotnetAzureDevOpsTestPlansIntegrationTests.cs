using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace Dotnet.AzureDevOps.TestPlans.IntegrationTests
{
    [TestType(TestType.Integration)]
    public class DotnetAzureDevOpsTestPlansIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly TestPlansClient _testPlansClient;
        private readonly List<int> _createdPlanIds = [];

        public DotnetAzureDevOpsTestPlansIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();

            _testPlansClient = new TestPlansClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        [Fact]
        public async Task PlanCrud_SucceedsAsync()
        {
            var create = new TestPlanCreateOptions
            {
                Name = $"it-plan-{UtcStamp()}",
                Description = "Created by integration test"
            };

            int planId = await _testPlansClient.CreateTestPlanAsync(create);
            _createdPlanIds.Add(planId);

            TestPlan? read = await _testPlansClient.GetTestPlanAsync(planId);
            Assert.NotNull(read);
            Assert.Equal(create.Name, read!.Name);

            IReadOnlyList<TestPlan> list = await _testPlansClient.ListTestPlansAsync();
            Assert.Contains(list, p => p.Id == planId);

            await _testPlansClient.DeleteTestPlanAsync(planId);
            _createdPlanIds.Remove(planId);

            TestPlan? afterDelete = await _testPlansClient.GetTestPlanAsync(planId);
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task SuiteCrud_SucceedsAsync()
        {
            int planId = await _testPlansClient.CreateTestPlanAsync(new TestPlanCreateOptions
            {
                Name = $"it-plan-{UtcStamp()}"
            });
            _createdPlanIds.Add(planId);

            // Retrieve root suite of the test plan (usually ID = 1, but safer to fetch dynamically)
            TestSuite rootSuite = await _testPlansClient.GetRootSuiteAsync(planId);

            var suiteCreate = new TestSuiteCreateOptions
            {
                Name = $"it-suite-{UtcStamp()}",
                ParentSuite = new TestSuiteReference
                {
                    Id = rootSuite.Id
                }
            };

            int suiteId = await _testPlansClient.CreateTestSuiteAsync(planId, suiteCreate);

            IReadOnlyList<TestSuite> suites = await _testPlansClient.ListTestSuitesAsync(planId);
            Assert.Contains(suites, s => s.Id == suiteId);
        }

        [Fact]
        public async Task TestCaseCreateAndAdd_SucceedsAsync()
        {
            int planId = await _testPlansClient.CreateTestPlanAsync(new TestPlanCreateOptions
            {
                Name = $"it-plan-{UtcStamp()}"
            });
            _createdPlanIds.Add(planId);

            TestSuite rootSuite = await _testPlansClient.GetRootSuiteAsync(planId);

            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem? testCase = await _testPlansClient.CreateTestCaseAsync(
                new TestCaseCreateOptions
                {
                    Title = "Integration Test Case",
                    Project = _azureDevOpsConfiguration.ProjectName
                });
            Assert.NotNull(testCase);

            await _testPlansClient.AddTestCasesAsync(planId, rootSuite.Id, [testCase!.Id!.Value]);

            Microsoft.VisualStudio.Services.WebApi.PagedList<TestCase> list = await _testPlansClient.ListTestCasesAsync(planId, rootSuite.Id);

            Assert.Contains(list, w => w.workItem.Id == testCase.Id);
        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach(int id in _createdPlanIds.AsEnumerable().Reverse())
            {
                try
                { await _testPlansClient.DeleteTestPlanAsync(id); }
                catch
                {
                    // Ignore errors during cleanup; the test plan may have already been deleted or not exist
                }
            }
        }
    }
}
