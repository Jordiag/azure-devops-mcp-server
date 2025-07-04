namespace Dotnet.AzureDevOps.Tests.Common
{
    public class AzureDevOpsConfiguration
    {
        public string OrganizationUrl { get; }
        public string ProjectName { get; }
        public string PersonalAccessToken { get; }
        public string ProjectId { get; }
        public string RepositoryId { get; }
        public string BuildBranch { get; }
        public string? CommitSha { get; }
        public string PipelineId { get; }
        public string SrcBranch { get; }
        public string TargetBranch { get; }
        public string BotUserEmail { get; }
        public string RepoName { get; }
        public string MainBranchName { get; }
        public string RepoId { get; }

        public AzureDevOpsConfiguration()
        {
            OrganizationUrl = GetEnv("AZURE_DEVOPS_ORG_URL");
            ProjectName = GetEnv("AZURE_DEVOPS_PROJECT_NAME");
            PersonalAccessToken = GetEnv("AZURE_DEVOPS_PAT");
            ProjectId = GetEnv("AZURE_DEVOPS_PROJECT_ID");
            RepositoryId = GetEnv("AZURE_DEVOPS_REPOSITORY_ID");
            BuildBranch = GetEnv("AZURE_DEVOPS_BUILD_BRANCH");
            CommitSha = GetEnv("AZURE_DEVOPS_COMMIT_SHA") == "null" ? null : GetEnv("AZURE_DEVOPS_COMMIT_SHA");
            PipelineId = GetEnv("AZURE_DEVOPS_PIPELINE_ID");
            SrcBranch = GetEnv("AZURE_DEVOPS_SRC_BRANCH") ?? "refs/heads/feature/integration-test";
            TargetBranch = GetEnv("AZURE_DEVOPS_TARGET_BRANCH") ?? "refs/heads/main";
            BotUserEmail = GetEnv("AZURE_DEVOPS_BOT_USER_EMAIL") ?? string.Empty;
            RepoName = GetEnv("AZURE_DEVOPS_REPO_NAME");
            RepoId = GetEnv("AZURE_DEVOPS_REPO_ID");
            MainBranchName = GetEnv("AZURE_DEVOPS_MAIN_BRANCH_NAME");
        }

        private static string GetEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new ArgumentException($"{name} environment variable is missing.");
    }
}
