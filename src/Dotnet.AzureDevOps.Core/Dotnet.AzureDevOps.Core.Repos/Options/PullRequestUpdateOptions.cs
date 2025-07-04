namespace Dotnet.AzureDevOps.Core.Repos.Options
{
    public record PullRequestUpdateOptions
    {
        public string? Title { get; init; }

        public string? Description { get; init; }

        public bool? IsDraft { get; init; }

        public IEnumerable<string>? ReviewerIds { get; init; }   // AAD object IDs or descriptor GUIDs
    }
}