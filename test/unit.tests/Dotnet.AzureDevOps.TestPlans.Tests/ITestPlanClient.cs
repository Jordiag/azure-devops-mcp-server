using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;

namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Interface for test plan operations to enable easier unit testing
/// This is a simplified interface that only includes the methods we need for testing
/// </summary>
public interface ITestPlanClient
{
    Task<List<TestCase>> AddTestCasesToSuiteAsync(
        List<SuiteTestCaseCreateUpdateParameters> suiteTestCaseCreateUpdateParameters,
        string project,
        int planId,
        int suiteId,
        CancellationToken cancellationToken = default);

    Task<TestPlan> CreateTestPlanAsync(
        TestPlanCreateParams testPlanCreateParams,
        string project,
        CancellationToken cancellationToken = default);

    Task<TestPlan> GetTestPlanByIdAsync(
        string project,
        int planId,
        CancellationToken cancellationToken = default);

    Task<PagedList<TestPlan>> GetTestPlansAsync(
        string project,
        CancellationToken cancellationToken = default);

    Task DeleteTestPlanAsync(
        string project,
        int planId,
        CancellationToken cancellationToken = default);

    Task<TestSuite> CreateTestSuiteAsync(
        TestSuiteCreateParams testSuiteCreateParams,
        string project,
        int planId,
        CancellationToken cancellationToken = default);

    Task<PagedList<TestSuite>> GetTestSuitesForPlanAsync(
        string project,
        int planId,
        bool asTreeView,
        CancellationToken cancellationToken = default);

    Task<PagedList<TestCase>> GetTestCaseListAsync(
        string project,
        int planId,
        int suiteId,
        CancellationToken cancellationToken = default);
}
