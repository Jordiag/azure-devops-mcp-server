using Microsoft.TeamFoundation.SourceControl.WebApi;
namespace Dotnet.AzureDevOps.Core.Repos.Options
{
    public record PullRequestStatusOptions
    {
        public string ContextName { get; init; } = "ci/some-check";

        public string ContextGenre { get; init; } = "continuous-integration";

        public GitStatusState State { get; init; } = GitStatusState.Succeeded;

        public string? Description { get; init; }

        public string? TargetUrl { get; init; }
    }
}