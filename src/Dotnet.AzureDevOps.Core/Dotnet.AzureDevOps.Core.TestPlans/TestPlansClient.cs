using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.TestResults.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.Linq;
using WorkItem = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using PointAssignment = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.PointAssignment;

namespace Dotnet.AzureDevOps.Core.TestPlans;

public class TestPlansClient : ITestPlansClient
{
    private readonly string _projectName;
    private readonly TestPlanHttpClient _testPlanClient;
    private readonly VssConnection _connection;

    public TestPlansClient(string organizationUrl, string projectName, string personalAccessToken)
    {
        _projectName = projectName;

        VssBasicCredential credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        _connection = new VssConnection(new Uri(organizationUrl), credentials);
        _testPlanClient = _connection.GetClient<TestPlanHttpClient>();
    }

    public async Task<AzureDevOpsActionResult<int>> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            TestPlanCreateParams createParameters = new TestPlanCreateParams
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

            return AzureDevOpsActionResult<int>.Success(plan.Id);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<TestPlan?>> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            TestPlan plan = await _testPlanClient.GetTestPlanByIdAsync(
                project: _projectName,
                planId: testPlanId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<TestPlan?>.Success(plan);
        }
        catch(VssServiceException)
        {
            return AzureDevOpsActionResult<TestPlan?>.Success(null);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestPlan?>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<TestPlan>>> ListTestPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestPlan> plans = await _testPlanClient.GetTestPlansAsync(
                project: _projectName,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<IReadOnlyList<TestPlan>>.Success(plans);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<TestPlan>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _testPlanClient.DeleteTestPlanAsync(
                project: _projectName,
                planId: testPlanId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<int>> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            TestSuiteCreateParams createParameters = new TestSuiteCreateParams
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

            return AzureDevOpsActionResult<int>.Success(suite.Id);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<TestSuite>>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestSuite> suites = await _testPlanClient.GetTestSuitesForPlanAsync(
                project: _projectName,
                planId: testPlanId,
                asTreeView: false,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<IReadOnlyList<TestSuite>>.Success(suites);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<TestSuite>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var references = testCaseIds.Select(id => new WorkItem { Id = id }).ToList();
            var existingTestCases = new List<SuiteTestCaseCreateUpdateParameters>();

            foreach(WorkItem workItem in references)
            {
                SuiteTestCaseCreateUpdateParameters suiteTestCase = new SuiteTestCaseCreateUpdateParameters
                {
                    workItem = new WorkItem { Id = workItem.Id },
                    PointAssignments = []
                };
                existingTestCases.Add(suiteTestCase);
            }

            await _testPlanClient.AddTestCasesToSuiteAsync(
                suiteTestCaseCreateUpdateParameters: existingTestCases,
                project: _projectName,
                planId: testPlanId,
                suiteId: testSuiteId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem?>> CreateTestCaseAsync(TestCaseCreateOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var workItemTracking = new WorkItemTrackingHttpClient(_connection.Uri, _connection.Credentials);

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = options.Title }
            };

            if(!string.IsNullOrWhiteSpace(options.Steps))
            {
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.TCM.Steps",
                    Value = options.Steps
                });
            }

            if(options.Priority.HasValue)
            {
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Priority",
                    Value = options.Priority.Value
                });
            }

            if(!string.IsNullOrWhiteSpace(options.AreaPath))
            {
                patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.AreaPath", Value = options.AreaPath });
            }

            if(!string.IsNullOrWhiteSpace(options.IterationPath))
            {
                patch.Add(new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.IterationPath", Value = options.IterationPath });
            }

            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem result = await workItemTracking.CreateWorkItemAsync(
                patch,
                options.Project,
                "Test Case",
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem?>.Success(result);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem?>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<PagedList<TestCase>>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestCase> testCases = await _testPlanClient.GetTestCaseListAsync(
                _projectName,
                testPlanId,
                testSuiteId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<PagedList<TestCase>>.Success(testCases);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<PagedList<TestCase>>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<TestResultsDetails>> GetTestResultsForBuildAsync(string projectName, int buildId, CancellationToken cancellationToken = default)
    {
        try
        {
            TestResultsHttpClient testResultsClient = _connection.GetClient<TestResultsHttpClient>();
            TestResultsDetails details = await testResultsClient.GetTestResultDetailsForBuildAsync(
                projectName,
                buildId,
                cancellationToken: cancellationToken);
            return AzureDevOpsActionResult<TestResultsDetails>.Success(details);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestResultsDetails>.Failure(ex);
        }
    }

    public async Task<AzureDevOpsActionResult<TestSuite>> GetRootSuiteAsync(int planId)
    {
        try
        {
            AzureDevOpsActionResult<IReadOnlyList<TestSuite>> suitesResult = await ListTestSuitesAsync(planId);
            if (!suitesResult.IsSuccessful || suitesResult.Value == null)
                return AzureDevOpsActionResult<TestSuite>.Failure(suitesResult.ErrorMessage ?? $"Unable to list suites for plan {planId}.");

            IReadOnlyList<TestSuite> suites = suitesResult.Value;
            TestSuite? root = suites.FirstOrDefault(suite => suite.ParentSuite == null || suite.ParentSuite.Id != -1);
            return root is null
                ? AzureDevOpsActionResult<TestSuite>.Failure($"No root suite found for test plan {planId}.")
                : AzureDevOpsActionResult<TestSuite>.Success(root);
        }
        catch (Exception ex)
        {
            return AzureDevOpsActionResult<TestSuite>.Failure(ex);
        }
    }
}
