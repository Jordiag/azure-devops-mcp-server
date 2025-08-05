using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Dashboards.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public interface IDashboardClient
    {
        Task<AzureDevOpsActionResult<IReadOnlyList<Dashboard>>> ListDashboardsAsync(CancellationToken cancellationToken = default);
        Task<AzureDevOpsActionResult<Dashboard>> GetDashboardAsync(Guid dashboardId, string teamName, CancellationToken cancellationToken = default);
    }
}
