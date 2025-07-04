using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace Dotnet.AzureDevOps.Core.TestPlans.Options;

public record TestSuiteCreateOptions
{
    public string Name { get; init; } = string.Empty;
    public required TestSuiteReference ParentSuite { get;  init; }
}
