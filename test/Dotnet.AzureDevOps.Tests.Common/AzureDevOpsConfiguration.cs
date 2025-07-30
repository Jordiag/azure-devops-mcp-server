using Microsoft.Extensions.Configuration;

namespace Dotnet.AzureDevOps.Tests.Common
{
    public class AzureDevOpsConfiguration
    {
        public string Organisation { get; private set; } = null!;
        public string OrganisationUrl { get; private set; } = null!;
        public string ProjectName { get; private set; } = null!;
        public string PersonalAccessToken { get; private set; } = null!;
        public string ProjectId { get; private set; } = null!;
        public string RepositoryId { get; private set; } = null!;
        public string BuildBranch { get; private set; } = null!;
        public string? CommitSha { get; private set; }
        public string PipelineId { get; private set; } = null!;
        public string SrcBranch { get; private set; } = null!;
        public string TargetBranch { get; private set; } = null!;
        public string BotUserEmail { get; private set; } = null!;
        public string RepoName { get; private set; } = null!;
        public string MainBranchName { get; private set; } = null!;
        public string RepoId { get; private set; } = null!;
        public int PipelineDefinitionId { get; private set; }

        private AzureDevOpsConfiguration() { }
        public static AzureDevOpsConfiguration FromEnvironment()
            => FromConfiguration(TestConfiguration.Configuration);

        public static AzureDevOpsConfiguration FromConfiguration(IConfiguration config) 
            => new()
            {
                Organisation = config.GetRequiredSection("AZURE_DEVOPS_ORG").Value!,
                OrganisationUrl = config.GetRequiredSection("AZURE_DEVOPS_ORG_URL").Value!,
                ProjectName = config.GetRequiredSection("AZURE_DEVOPS_PROJECT_NAME").Value!,
                PersonalAccessToken = config.GetRequiredSection("AZURE_DEVOPS_PAT").Value!,
                ProjectId = config.GetRequiredSection("AZURE_DEVOPS_PROJECT_ID").Value!,
                RepositoryId = config.GetRequiredSection("AZURE_DEVOPS_REPOSITORY_ID").Value!,
                BuildBranch = config.GetRequiredSection("AZURE_DEVOPS_BUILD_BRANCH").Value!,
                CommitSha = config.GetSection("AZURE_DEVOPS_COMMIT_SHA").Value ?? config.GetRequiredSection("AZURE_DEVOPS_COMMIT_SHA").Value!,
                PipelineId = config.GetRequiredSection("AZURE_DEVOPS_PIPELINE_ID").Value!,
                SrcBranch = config.GetSection("AZURE_DEVOPS_SRC_BRANCH").Value! ?? "refs/heads/feature/integration-test",
                TargetBranch = config.GetSection("AZURE_DEVOPS_TARGET_BRANCH").Value! ?? "refs/heads/main",
                BotUserEmail = config.GetSection("AZURE_DEVOPS_BOT_USER_EMAIL").Value! ?? string.Empty,
                RepoName = config.GetRequiredSection("AZURE_DEVOPS_REPO_NAME").Value!,
                RepoId = config.GetRequiredSection("AZURE_DEVOPS_REPO_ID").Value!,
                MainBranchName = config.GetRequiredSection("AZURE_DEVOPS_MAIN_BRANCH_NAME").Value!,
                PipelineDefinitionId = 85
            };
    }
}
