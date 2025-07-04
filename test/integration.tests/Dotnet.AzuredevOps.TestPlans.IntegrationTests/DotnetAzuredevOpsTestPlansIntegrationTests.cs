using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Core.TestPlans;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace Dotnet.AzuredevOps.TestPlans.IntegrationTests
{
    [ExcludeFromCodeCoverage]
    public class DotnetAzuredevOpsTestPlansIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly TestPlansClient _testPlansClient;
        private readonly List<int> _createdPlanIds = [];

        public DotnetAzuredevOpsTestPlansIntegrationTests()
        {
            _azureDevOpsConfiguration = new AzureDevOpsConfiguration();

            _testPlansClient = new TestPlansClient(
                _azureDevOpsConfiguration.OrganisationUrl,
                _azureDevOpsConfiguration.ProjectName,
                _azureDevOpsConfiguration.PersonalAccessToken);
        }

        /*──────────────────────────── Plan CRUD ───────────────────────────*/

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

        /*─────────────────────────── Suite CRUD ───────────────────────────*/

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

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        /*──────────────────── IAsyncLifetime ───────────────────*/

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
