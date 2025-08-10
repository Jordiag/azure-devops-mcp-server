using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<List<GitRefUpdateResult>>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha, CancellationToken cancellationToken = default)
        {
            try
            {
                var refUpdate = new GitRefUpdate
                {
                    Name = newRefName,
                    OldObjectId = "0000000000000000000000000000000000000000",
                    NewObjectId = baseCommitSha
                };

                List<GitRefUpdateResult> result = await _gitHttpClient.UpdateRefsAsync(
                    refUpdates: new[] { refUpdate },
                    repositoryId: repositoryId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<List<GitRefUpdateResult>>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<List<GitRefUpdateResult>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListBranchesAsync(string repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    filter: "heads/",
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Success(refs, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListMyBranchesAsync(string repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    includeMyBranches: true,
                    latestStatusesOnly: true,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Success(refs, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRef>> GetBranchAsync(string repositoryId, string branchName, CancellationToken cancellationToken = default)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    filter: $"heads/{branchName}",
                    cancellationToken: cancellationToken);

                if(refs.Count == 0)
                    return AzureDevOpsActionResult<GitRef>.Failure($"Branch '{branchName}' not found.", Logger);

                return AzureDevOpsActionResult<GitRef>.Success(refs[0], Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRef>.Failure(ex, Logger);
            }
        }
    }
}

