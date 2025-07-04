namespace Dotnet.AzureDevOps.Core.Repos.Options
{
    /// <summary> Describes a new top-level comment thread. </summary>
    public record CommentThreadOptions
    {
        public string RepositoryId { get; init; } = string.Empty;

        public int PullRequestId { get; init; }

        public string Comment { get; init; } = string.Empty;

        public bool IsLeftSide { get; init; } = false;           // diff side

        public string? FilePath { get; init; }                   // leave null for PR-wide comment
    }

}