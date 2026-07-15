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

        private async Task EnsureBranchDeletedAsync(
            string repositoryId,
            string sourceRefName,
            CancellationToken cancellationToken)
        {
            const string refsPrefix = "refs/";
            string filter = sourceRefName.StartsWith(refsPrefix, StringComparison.Ordinal)
                ? sourceRefName[refsPrefix.Length..]
                : sourceRefName;
            List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                repositoryId: repositoryId,
                project: ProjectName,
                filter: filter,
                cancellationToken: cancellationToken);
            GitRef? sourceRef = refs.SingleOrDefault(candidate =>
                candidate.Name.Equals(sourceRefName, StringComparison.Ordinal));
            if(sourceRef is null)
                return;

            GitRefUpdate deleteUpdate = new()
            {
                Name = sourceRef.Name,
                OldObjectId = sourceRef.ObjectId,
                NewObjectId = "0000000000000000000000000000000000000000"
            };
            List<GitRefUpdateResult> results = await _gitHttpClient.UpdateRefsAsync(
                refUpdates: [deleteUpdate],
                repositoryId: repositoryId,
                project: ProjectName,
                cancellationToken: cancellationToken);
            if(results.Count != 1 || !results[0].Success)
                throw new InvalidOperationException($"Pull request completed, but source branch '{sourceRefName}' could not be deleted.");

            List<GitRef> remainingRefs = await _gitHttpClient.GetRefsAsync(
                repositoryId: repositoryId,
                project: ProjectName,
                filter: filter,
                cancellationToken: cancellationToken);
            if(remainingRefs.Any(candidate => candidate.Name.Equals(sourceRefName, StringComparison.Ordinal)))
                throw new InvalidOperationException($"Pull request completed, but source branch '{sourceRefName}' still exists after deletion.");
        }
    }
}

