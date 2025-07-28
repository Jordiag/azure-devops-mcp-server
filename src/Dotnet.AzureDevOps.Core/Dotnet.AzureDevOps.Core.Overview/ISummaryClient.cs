using Microsoft.TeamFoundation.Core.WebApi;

namespace Dotnet.AzureDevOps.Core.Overview
{
    public interface ISummaryClient
    {
        Task<TeamProject?> GetProjectSummaryAsync(CancellationToken cancellationToken = default);
    }
}
