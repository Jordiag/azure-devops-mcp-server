using Dotnet.AzureDevOps.Core.Common;

namespace Dotnet.AzureDevOps.Core.Repos;

public interface IIdentityClient
{
    Task<AzureDevOpsActionResult<(string localId, string displayName)>> GetUserLocalIdFromEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}