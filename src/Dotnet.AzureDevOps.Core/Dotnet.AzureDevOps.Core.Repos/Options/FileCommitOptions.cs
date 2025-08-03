namespace Dotnet.AzureDevOps.Core.Repos.Options
{
    public class FileCommitOptions
    {
        public string RepositoryName { get; init; } = string.Empty;

        public string BranchName { get; init; } = string.Empty;

        public string CommitMessage { get; init; } = string.Empty;

        public string FilePath { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;
    }
}