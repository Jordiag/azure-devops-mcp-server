using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview;

public interface ISummaryClient
{
    Task<AzureDevOpsActionResult<TeamProject>> GetProjectSummaryAsync(CancellationToken cancellationToken = default);
}
