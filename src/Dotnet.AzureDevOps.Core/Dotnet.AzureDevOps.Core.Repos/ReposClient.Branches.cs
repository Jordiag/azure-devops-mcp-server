using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<List<GitRefUpdateResult>>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha)
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
                    project: _projectName);

                return AzureDevOpsActionResult<List<GitRefUpdateResult>>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<List<GitRefUpdateResult>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListBranchesAsync(string repositoryId)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    filter: "heads/");

                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Success(refs, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<GitRef>>> ListMyBranchesAsync(string repositoryId)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    includeMyBranches: true,
                    latestStatusesOnly: true);

                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Success(refs, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<GitRef>>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRef>> GetBranchAsync(string repositoryId, string branchName)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    filter: $"heads/{branchName}");

                if(refs.Count == 0)
                    return AzureDevOpsActionResult<GitRef>.Failure($"Branch '{branchName}' not found.", _logger);

                return AzureDevOpsActionResult<GitRef>.Success(refs[0], _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRef>.Failure(ex, _logger);
            }
        }
    }
}
