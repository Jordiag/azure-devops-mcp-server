using Dotnet.AzureDevOps.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.TestPlans.Tests;

/// <summary>
/// Testable version of TestPlansClient that uses ITestPlanClient interface for easier mocking
/// </summary>
public class TestPlansClientWithInterface
{
    private readonly ITestPlanClient _testPlanClient;
    private readonly string _projectName;
    private readonly ILogger? _logger;

    public TestPlansClientWithInterface(ITestPlanClient testPlanClient, string projectName, ILogger? logger = null)
    {
        _testPlanClient = testPlanClient;
        _projectName = projectName;
        _logger = logger;
    }

    public async Task<AzureDevOpsActionResult<bool>> AddTestCasesAsync(int testPlanId, int testSuiteId, IReadOnlyList<int> testCaseIds, CancellationToken cancellationToken = default)
    {
        try
        {
            List<WorkItem> references =
                testCaseIds.Select(id => new WorkItem { Id = id }).ToList();
            List<SuiteTestCaseCreateUpdateParameters> existingTestCases = new List<SuiteTestCaseCreateUpdateParameters>();

            foreach(WorkItem workItem in references)
            {
                SuiteTestCaseCreateUpdateParameters suiteTestCase = new SuiteTestCaseCreateUpdateParameters
                {
                    workItem = new WorkItem { Id = workItem.Id },
                    PointAssignments = new List<Configuration>()
                };
                existingTestCases.Add(suiteTestCase);
            }

            await _testPlanClient.AddTestCasesToSuiteAsync(
                suiteTestCaseCreateUpdateParameters: existingTestCases,
                project: _projectName,
                planId: testPlanId,
                suiteId: testSuiteId,
                cancellationToken: cancellationToken);

            return AzureDevOpsActionResult<bool>.Success(true, _logger);
        }
        catch(Exception ex)
        {
            return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
        }
    }
}
