using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<Guid>> CreateRepositoryAsync(string newRepositoryName, CancellationToken cancellationToken = default)
        {
            try
            {
                Guid id = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var newRepositoryOptions = new GitRepositoryCreateOptions { Name = newRepositoryName };
                    GitRepository repo = await _gitHttpClient.CreateRepositoryAsync(
                    gitRepositoryToCreate: newRepositoryOptions,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );
                    return repo.Id;
                }, "CreateRepository", OperationType.Create);

                return AzureDevOpsActionResult<Guid>.Success(id, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool deleted = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    await _gitHttpClient.DeleteRepositoryAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );
                return true;
                }, "DeleteRepository", OperationType.Delete);

                return AzureDevOpsActionResult<bool>.Success(deleted, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                GitRepository repo = await _gitHttpClient.GetRepositoryAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<GitRepository>.Success(repo, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryByNameAsync(string repositoryName, CancellationToken cancellationToken = default)
        {
            try
            {
                GitRepository repo = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.GetRepositoryAsync(
                    repositoryId: repositoryName,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );
                }, "GetRepositoryByName", OperationType.Read);

                return AzureDevOpsActionResult<GitRepository>.Success(repo, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRepository>>> ListRepositoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitRepository> repos = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.GetRepositoriesAsync(project: ProjectName, cancellationToken: cancellationToken);
                }, "ListRepositories", OperationType.Read);

                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Success(repos, Logger);
            }
            catch (Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Failure(ex, Logger);
            }
        }
    }
}

