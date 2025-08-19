using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<GitCommitDiffs>> GetCommitDiffAsync(string repositoryId, string baseSha, string targetSha, CancellationToken cancellationToken = default)
        {
            try
            {
                GitCommitDiffs diffs = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var baseDesc = new GitBaseVersionDescriptor { Version = baseSha, VersionType = GitVersionType.Commit };
                    var targetDesc = new GitTargetVersionDescriptor { Version = targetSha, VersionType = GitVersionType.Commit };
                    return await _gitHttpClient.GetCommitDiffsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    diffCommonCommit: true,
                    baseVersionDescriptor: baseDesc,
                    targetVersionDescriptor: targetDesc,
                    cancellationToken: cancellationToken);
                }, "GetCommitDiff", OperationType.Read);

                return AzureDevOpsActionResult<GitCommitDiffs>.Success(diffs, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<GitCommitDiffs>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> GetLatestCommitsAsync(string projectName, string repositoryName, string branchName, int top = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitCommitRef> commits = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var searchCriteria = new GitQueryCommitsCriteria
                    {
                        IncludeWorkItems = false,
                        ItemVersion = new GitVersionDescriptor { Version = branchName, VersionType = GitVersionType.Branch },
                        Top = top
                    };
                    return await _gitHttpClient.GetCommitsAsync(
                    repositoryId: repositoryName,
                    searchCriteria: searchCriteria,
                    project: projectName,
                    cancellationToken: cancellationToken);

                }, "GetLatestCommits", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Success(commits, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>> SearchCommitsAsync(string repositoryId, GitQueryCommitsCriteria searchCriteria, int top = 100, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitCommitRef> commits = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    searchCriteria.Top = top;
                    return await _gitHttpClient.GetCommitsAsync(
                    repositoryId: repositoryId,
                    searchCriteria: searchCriteria,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "SearchCommits", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Success(commits, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitCommitRef>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<string>> CommitAddFileAsync(FileCommitOptions fileCommitOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                string commitId = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    GitRef branch = await _gitHttpClient.GetRefsAsync(
                    repositoryId: fileCommitOptions.RepositoryName,
                    project: ProjectName,
                    filter: $"heads/{fileCommitOptions.BranchName}",
                    cancellationToken: cancellationToken)
                    .ContinueWith(task => task.Result.Single());


                    var change = new GitChange
                    {
                        ChangeType = VersionControlChangeType.Add,
                        Item = new GitItem { Path = fileCommitOptions.FilePath },
                        NewContent = new ItemContent { Content = fileCommitOptions.Content, ContentType = ItemContentType.RawText }
                    };

                    var commit = new GitCommitRef { Comment = fileCommitOptions.CommitMessage, Changes = [change] };
                    var referenceUpdate = new GitRefUpdate { Name = $"refs/heads/{fileCommitOptions.BranchName}", OldObjectId = branch.ObjectId };
                    var push = new GitPush { RefUpdates = [referenceUpdate], Commits = [commit] };

                    GitPush result = await _gitHttpClient.CreatePushAsync(push, project: ProjectName, repositoryId: fileCommitOptions.RepositoryName, userState: null, cancellationToken: cancellationToken);
                    return result.Commits.Last().CommitId;
                }, "CommitAddFile", OperationType.Create);

                return AzureDevOpsActionResult<string>.Success(commitId, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex, Logger);
            }
        }
    }
}

