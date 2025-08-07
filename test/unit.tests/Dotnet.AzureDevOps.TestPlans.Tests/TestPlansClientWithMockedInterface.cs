using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.TestPlans.Options;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;

namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Testable version of TestPlansClient that uses ITestPlanClient interface for easier mocking
/// This contains simplified implementations of all the TestPlans methods for unit testing
/// </summary>
public class TestPlansClientWithMockedInterface
{
    private readonly ITestPlanClient _testPlanClient;
    private readonly string _projectName;
    private readonly ILogger? _logger;

    public TestPlansClientWithMockedInterface(ITestPlanClient testPlanClient, string projectName, ILogger? logger = null)
    {
        _testPlanClient = testPlanClient;
        _projectName = projectName;
        _logger = logger;
    }

    public async Task<AzureDevOpsActionResult<int>> CreateTestPlanAsync(TestPlanCreateOptions testPlanCreateOptions, CancellationToken cancellationToken = default)
    {
        try
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

            TestPlan plan = await _testPlanClient.CreateTestPlanAsync(createParameters, _projectName, cancellationToken);
            return AzureDevOpsActionResult<int>.Success(plan.Id, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<TestPlan>> GetTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            TestPlan plan = await _testPlanClient.GetTestPlanByIdAsync(_projectName, testPlanId, cancellationToken);
            return AzureDevOpsActionResult<TestPlan>.Success(plan, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestPlan>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<TestPlan>>> ListTestPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestPlan> plans = await _testPlanClient.GetTestPlansAsync(_projectName, cancellationToken);
            return AzureDevOpsActionResult<IReadOnlyList<TestPlan>>.Success(plans, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<TestPlan>>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<bool>> DeleteTestPlanAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _testPlanClient.DeleteTestPlanAsync(_projectName, testPlanId, cancellationToken);
            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<int>> CreateTestSuiteAsync(int testPlanId, TestSuiteCreateOptions testSuiteCreateOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            var createParameters = new TestSuiteCreateParams
            {
                Name = testSuiteCreateOptions.Name,
                SuiteType = TestSuiteType.StaticTestSuite,
                ParentSuite = testSuiteCreateOptions.ParentSuite
            };

            TestSuite suite = await _testPlanClient.CreateTestSuiteAsync(createParameters, _projectName, testPlanId, cancellationToken);
            return AzureDevOpsActionResult<int>.Success(suite.Id, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<int>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<IReadOnlyList<TestSuite>>> ListTestSuitesAsync(int testPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestSuite> suites = await _testPlanClient.GetTestSuitesForPlanAsync(_projectName, testPlanId, false, cancellationToken);
            return AzureDevOpsActionResult<IReadOnlyList<TestSuite>>.Success(suites, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<IReadOnlyList<TestSuite>>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<PagedList<TestCase>>> ListTestCasesAsync(int testPlanId, int testSuiteId, CancellationToken cancellationToken = default)
    {
        try
        {
            PagedList<TestCase> testCases = await _testPlanClient.GetTestCaseListAsync(_projectName, testPlanId, testSuiteId, cancellationToken);
            return AzureDevOpsActionResult<PagedList<TestCase>>.Success(testCases, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<PagedList<TestCase>>.Failure(ex, _logger);
        }
    }

    public async Task<AzureDevOpsActionResult<TestSuite>> GetRootSuiteAsync(int planId)
    {
        try
        {
            AzureDevOpsActionResult<IReadOnlyList<TestSuite>> suitesResult = await ListTestSuitesAsync(planId);
            if(!suitesResult.IsSuccessful || suitesResult.Value == null)
                return AzureDevOpsActionResult<TestSuite>.Failure(suitesResult.ErrorMessage ?? $"Unable to list suites for plan {planId}.", _logger);

            IReadOnlyList<TestSuite> suites = suitesResult.Value;
            TestSuite? root = suites.FirstOrDefault(suite => suite.ParentSuite == null || suite.ParentSuite.Id != -1);

            return root is null
                ? AzureDevOpsActionResult<TestSuite>.Failure($"No root suite found for test plan {planId}.", _logger)
                : AzureDevOpsActionResult<TestSuite>.Success(root, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<TestSuite>.Failure(ex, _logger);
        }
    }
}
