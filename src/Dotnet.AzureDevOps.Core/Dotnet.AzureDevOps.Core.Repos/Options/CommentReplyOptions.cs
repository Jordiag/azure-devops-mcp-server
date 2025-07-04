namespace Dotnet.AzureDevOps.Core.Repos.Options
{
    /// <summary> Reply to an existing comment thread. </summary>
    public record CommentReplyOptions
    {
        public string Repository { get; init; } = string.Empty;

        public int PullRequestId { get; init; }

        public int ThreadId { get; init; }

        public string Comment { get; init; } = string.Empty;

        public bool ResolveThread { get; init; } = false;
    }
}