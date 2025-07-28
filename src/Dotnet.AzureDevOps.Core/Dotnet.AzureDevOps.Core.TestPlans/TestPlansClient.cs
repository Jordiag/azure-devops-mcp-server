using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.TestManagement.TestResults.WebApi;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.TestPlans;

public class TestPlansClient : ITestPlansClient
{
    private readonly string _projectName;
    private readonly TestPlanHttpClient _testPlanClient;
    private readonly VssConnection _connection;

    public TestPlansClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName;

        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        _connection = new VssConnection(new Uri(organizationUrl), credentials);
        _testPlanClient = _connection.GetClient<TestPlanHttpClient>();
    }

    public async Task<int> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default)
    {
        var createParameters = new TestPlanCreateParams
        {
            Name = testPlanCreateOptions.Name,
            AreaPath = testPlanCreateOptions.AreaPath,
            Iteration = testPlanCreateOptions.Iteration,
            StartDate = testPlanCreateOptions.StartDate,
            EndDate = testPlanCreateOptions.EndDate,
            Description = testPlanCreateOptions.Description
        };

        TestPlan plan = await _testPlanClient.CreateTestPlanAsync(
            testPlanCreateParams: createParameters,
            project: _projectName,
            cancellationToken: cancellationToken);
        return plan.Id;
    }

    public async Task<TestPlan?> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _testPlanClient.GetTestPlanByIdAsync(
                project: _projectName,
                planId: testPlanId,
                cancellationToken: cancellationToken);
        }
        catch(VssServiceException)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<TestPlan>> ListTestPlansAsync(CancellationToken cancellationToken = default)
    {
        List<TestPlan> plans = await _testPlanClient.GetTestPlansAsync(
            project: _projectName,
            cancellationToken: cancellationToken);
        return plans;
    }

    public Task DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default) =>
        _testPlanClient.DeleteTestPlanAsync(
            project: _projectName,
            planId: testPlanId,
            cancellationToken: cancellationToken);

    public async Task<int> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default)
    {
        var createParameters = new TestSuiteCreateParams
        {
            Name = testSuiteCreateOptions.Name,
            SuiteType = TestSuiteType.StaticTestSuite,
            ParentSuite = testSuiteCreateOptions.ParentSuite
        };

        TestSuite suite = await _testPlanClient.CreateTestSuiteAsync(
            testSuiteCreateParams: createParameters,
            project: _projectName,
            planId: testPlanId,
            cancellationToken: cancellationToken);
        return suite.Id;
    }

    public async Task<IReadOnlyList<TestSuite>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        List<TestSuite> suites = await _testPlanClient.GetTestSuitesForPlanAsync(
            project: _projectName,
            planId: testPlanId,
            asTreeView: false,
            cancellationToken: cancellationToken);
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

        await _testPlanClient.AddTestCasesToSuiteAsync(
            suiteTestCaseCreateUpdateParameters: existingTestCases,
            project: _projectName,
            planId: testPlanId,
            suiteId: testSuiteId,
            cancellationToken: cancellationToken);
    }

    public async Task<WorkItem?> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default)
    {
        var workItemTracking = new WorkItemTrackingHttpClient(_connection.Uri, _connection.Credentials);

        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = options.Title }
        };

        if (!string.IsNullOrWhiteSpace(options.Steps))
        {
            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.TCM.Steps",
                Value = options.Steps
            });
        }

        if (options.Priority.HasValue)
        {
            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/Microsoft.VSTS.Common.Priority",
                Value = options.Priority.Value
            });
        }

        if (!string.IsNullOrWhiteSpace(options.AreaPath))
        {
            patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.AreaPath", Value = options.AreaPath });
        }

        if (!string.IsNullOrWhiteSpace(options.IterationPath))
        {
            patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.IterationPath", Value = options.IterationPath });
        }

        WorkItem result = await workItemTracking.CreateWorkItemAsync(patch, options.Project, "Test Case", cancellationToken: cancellationToken);
        return result;
    }

    public Task<IReadOnlyList<WorkItem>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default) =>
        _testPlanClient.GetTestCaseListAsync(_projectName, testPlanId, testSuiteId, cancellationToken: cancellationToken)
            .ContinueWith(t => (IReadOnlyList<WorkItem>)t.Result);

    public Task<TestResultsDetails?> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default) =>
        _connection.GetClient<TestResultsHttpClient>().GetTestResultsDetailsForBuildAsync(projectName, buildId, cancellationToken: cancellationToken);

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
