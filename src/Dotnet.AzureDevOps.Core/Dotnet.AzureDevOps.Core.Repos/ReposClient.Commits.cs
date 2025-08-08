using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<GitCommitDiffs>> GetCommitDiffAsync(
            string repositoryId, string baseSha, string targetSha)
        {
            try
            {
                var baseDesc = new GitBaseVersionDescriptor
                {
                    Version = baseSha,
                    VersionType = GitVersionType.Commit
                };

                var targetDesc = new GitTargetVersionDescriptor
                {
                    Version = targetSha,
                    VersionType = GitVersionType.Commit
                };

                GitCommitDiffs result = await _gitHttpClient.GetCommitDiffsAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    diffCommonCommit: true,
                    baseVersionDescriptor: baseDesc,
                    targetVersionDescriptor: targetDesc);

                return AzureDevOpsActionResult<GitCommitDiffs>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitCommitDiffs>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1)
        {
            try
            {
                var searchCriteria = new GitQueryCommitsCriteria
                {
                    IncludeWorkItems = false,
                    ItemVersion = new GitVersionDescriptor
                    {
                        Version = branchName,
                        VersionType = GitVersionType.Branch
                    },
                    Top = top
                };

                IReadOnlyList<GitCommitRef> result = await _gitHttpClient.GetCommitsAsync(
                    repositoryId: repositoryName,
                    searchCriteria: searchCriteria,
                    project: projectName);

                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100)
        {
            try
            {
                searchCriteria.Top = top;

                IReadOnlyList<GitCommitRef> result = await _gitHttpClient.GetCommitsAsync(
                    repositoryId: repositoryId,
                    searchCriteria: searchCriteria,
                    project: _projectName);

                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<string>> CommitAddFileAsync(FileCommitOptions fileCommitOptions)
        {
            try
            {
                GitRef branch = await _gitHttpClient.GetRefsAsync(
                    repositoryId: fileCommitOptions.RepositoryName,
                    project: _projectName,
                    filter: $"heads/{fileCommitOptions.BranchName}")
                    .ContinueWith(task => task.Result.Single());

                var change = new GitChange
                {
                    ChangeType = VersionControlChangeType.Add,
                    Item = new GitItem { Path = fileCommitOptions.FilePath },
                    NewContent = new ItemContent
                    {
                        Content = fileCommitOptions.Content,
                        ContentType = ItemContentType.RawText
                    }
                };

                var commit = new GitCommitRef
                {
                    Comment = fileCommitOptions.CommitMessage,
                    Changes = [change]
                };

                var referenceUpdate = new GitRefUpdate
                {
                    Name = $"refs/heads/{fileCommitOptions.BranchName}",
                    OldObjectId = branch.ObjectId
                };

                var push = new GitPush
                {
                    RefUpdates = [referenceUpdate],
                    Commits = [commit]
                };

                GitPush result = await _gitHttpClient.CreatePushAsync(
                    push,
                    project: _projectName,
                    repositoryId: fileCommitOptions.RepositoryName,
                    userState: null
                );

                GitCommitRef pushedCommit = result.Commits.Last();
                return AzureDevOpsActionResult<string>.Success(pushedCommit.CommitId, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex, _logger);
            }
        }
    }
}
