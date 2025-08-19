using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Common.Services;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<List<GitRefUpdateResult>>> CreateBranchAsync(string repositoryId, string newRefName, string baseCommitSha, CancellationToken cancellationToken = default)
        {
            try
            {
                List<GitRefUpdateResult> result = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    var refUpdate = new GitRefUpdate { Name = newRefName, OldObjectId = "0000000000000000000000000000000000000000", NewObjectId = baseCommitSha };
                    return await _gitHttpClient.UpdateRefsAsync(
                    refUpdates: new[] { refUpdate },
                    repositoryId: repositoryId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);
                }, "CreateBranch", OperationType.Create);

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
                IReadOnlyList<GitRef> refs = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    filter: "heads/",
                    cancellationToken: cancellationToken);
                }, "ListBranches", OperationType.Read);

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
                IReadOnlyList<GitRef> refs = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    return await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    includeMyBranches: true,
                    latestStatusesOnly: true,
                    cancellationToken: cancellationToken);

                }, "ListMyBranches", OperationType.Read);

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
                GitRef branch = await ExecuteWithExceptionHandlingAsync(async () =>
                {
                    List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    filter: $"heads/{branchName}",
                    cancellationToken: cancellationToken);

                    if(refs.Count == 0)
                        throw new InvalidOperationException($"Branch '{branchName}' not found.");
                    return refs[0];
                }, "GetBranch", OperationType.Read);

                return AzureDevOpsActionResult<GitRef>.Success(branch, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRef>.Failure(ex, Logger);
            }
        }
    }
}

