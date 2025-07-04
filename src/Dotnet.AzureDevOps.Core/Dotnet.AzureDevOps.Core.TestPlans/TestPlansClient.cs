using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.TestPlans;

public class TestPlansClient : ITestPlansClient
{
    private readonly string _projectName;
    private readonly TestPlanHttpClient _testPlanClient;

    public TestPlansClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName;

        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        var connection = new VssConnection(new Uri(organizationUrl), credentials);
        _testPlanClient = connection.GetClient<TestPlanHttpClient>();
    }

    public async Task<int> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default)
    {
        var createParams = new TestPlanCreateParams
        {
            Name = testPlanCreateOptions.Name,
            AreaPath = testPlanCreateOptions.AreaPath,
            Iteration = testPlanCreateOptions.Iteration,
            StartDate = testPlanCreateOptions.StartDate,
            EndDate = testPlanCreateOptions.EndDate,
            Description = testPlanCreateOptions.Description
        };

        TestPlan plan = await _testPlanClient.CreateTestPlanAsync(createParams, _projectName, cancellationToken: cancellationToken);
        return plan.Id;
    }

    public async Task<TestPlan?> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _testPlanClient.GetTestPlanByIdAsync(_projectName, testPlanId, cancellationToken: cancellationToken);
        }
        catch(VssServiceException)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(CancellationToken cancellationToken = default)
    {
        List<TestPlan> plans = await _testPlanClient.GetTestPlansAsync(_projectName, cancellationToken: cancellationToken);
        return plans;
    }

    public Task DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default) =>
        _testPlanClient.DeleteTestPlanAsync(_projectName, testPlanId, cancellationToken: cancellationToken);

    public async Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default)
    {
        var createParams = new TestSuiteCreateParams
        {
            Name = testSuiteCreateOptions.Name,
            SuiteType = TestSuiteType.StaticTestSuite,
            ParentSuite = testSuiteCreateOptions.ParentSuite
        };

        TestSuite suite = await _testPlanClient.CreateTestSuiteAsync(createParams, _projectName, testPlanId, cancellationToken: cancellationToken);
        return suite.Id;
    }

    public async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        List<TestSuite> suites = await _testPlanClient.GetTestSuitesForPlanAsync(_projectName, testPlanId, asTreeView: false, cancellationToken: cancellationToken);
        return suites;
    }

    public async Task AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default)
    {
        IEnumerable<WorkItem> references = [.. testCaseIds.Select(id => new WorkItem { Id = id })];
        var existingTestCases = new List<SuiteTestCaseCreateUpdateParameters>();

        foreach(WorkItem workItem in references)
            existingTestCases.Add(new SuiteTestCaseCreateUpdateParameters
            {
                workItem = new WorkItem { Id = workItem.Id },
                PointAssignments = [], // Assuming no point assignments for simplicity
            });

        await _testPlanClient.AddTestCasesToSuiteAsync(existingTestCases, _projectName, testPlanId, testSuiteId, cancellationToken: cancellationToken);
    }

    public async Task<TestSuite> GetRootSuiteAsync(int planId)
    {
        // List all suites for the test plan
        IReadOnlyList<TestSuite> suites = await ListTestSuitesAsync(planId);

        // Root suite is the one without a parent, or sometimes has parent ID == -1
        TestSuite? root = 
            suites.FirstOrDefault(suite => suite.ParentSuite == null || suite.ParentSuite.Id == -1) ?? 
                throw new InvalidOperationException($"No root suite found for test plan {planId}.");

        return root;
    }
}
