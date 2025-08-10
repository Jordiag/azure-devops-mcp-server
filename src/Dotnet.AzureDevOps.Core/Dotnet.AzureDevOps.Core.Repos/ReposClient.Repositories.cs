using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<Guid>> CreateRepositoryAsync(string newRepositoryName, CancellationToken cancellationToken = default)
        {
            try
            {
                var newRepositoryOptions = new GitRepositoryCreateOptions { Name = newRepositoryName };

                GitRepository repo = await _gitHttpClient.CreateRepositoryAsync(
                    gitRepositoryToCreate: newRepositoryOptions,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<Guid>.Success(repo.Id, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _gitHttpClient.DeleteRepositoryAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<bool>.Success(true, Logger);
            }
            catch(Exception ex)
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
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryByNameAsync(string repositoryName, CancellationToken cancellationToken = default)
        {
            try
            {
                GitRepository repo = await _gitHttpClient.GetRepositoryAsync(
                    repositoryId: repositoryName,
                    project: ProjectName,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<GitRepository>.Success(repo, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRepository>>> ListRepositoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitRepository> result = await _gitHttpClient.GetRepositoriesAsync(project: ProjectName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Failure(ex, Logger);
            }
        }
    }
}

