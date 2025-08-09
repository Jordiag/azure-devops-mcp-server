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
                    project: _projectName,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<Guid>.Success(repo.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _gitHttpClient.DeleteRepositoryAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                GitRepository repo = await _gitHttpClient.GetRepositoryAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<GitRepository>.Success(repo, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRepository>> GetRepositoryByNameAsync(string repositoryName, CancellationToken cancellationToken = default)
        {
            try
            {
                GitRepository repo = await _gitHttpClient.GetRepositoryAsync(
                    repositoryId: repositoryName,
                    project: _projectName,
                    cancellationToken: cancellationToken
                );
                return AzureDevOpsActionResult<GitRepository>.Success(repo, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRepository>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRepository>>> ListRepositoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<GitRepository> result = await _gitHttpClient.GetRepositoriesAsync(project: _projectName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRepository>>.Failure(ex, _logger);
            }
        }
    }
}
