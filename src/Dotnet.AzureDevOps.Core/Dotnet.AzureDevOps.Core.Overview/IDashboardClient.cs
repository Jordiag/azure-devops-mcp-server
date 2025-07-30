using Microsoft.TeamFoundation.Dashboards.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public interface IDashboardClient
    {
        Task<IReadOnlyList<Dashboard>> ListDashboardsAsync(CancellationToken cancellationToken = default);
        Task<Dashboard?> GetDashboardAsync(Guid dashboardId, string teamName, CancellationToken cancellationToken = default);
    }
}
