using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos.Options;

public class GitPullRequestQueryResponse
{
    public List<Dictionary<string, List<GitPullRequest>>> Results { get; set; } = new();
}