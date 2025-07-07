namespace Dotnet.AzureDevOps.Core.Repos.Options
{
    public record CommentEditOptions
    {
        public string Repository { get; init; } = string.Empty;

        public int PullRequest { get; init; }

        public int ThreadId { get; init; }

        public int CommentId { get; init; }

        public string NewContent { get; init; } = string.Empty;
    }
}