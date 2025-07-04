using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public static class ReviewersClient
    {
        public static async Task<(string guid, string name)> GetUserIdFromEmailAsync(string OrganisationUrl, string pat, string email, CancellationToken cancellationToken = default)
        {
            var credentials = new VssBasicCredential(string.Empty, pat);
            var connection = new VssConnection(new Uri(OrganisationUrl), credentials);

            GraphHttpClient graphClient = await connection.GetClientAsync<GraphHttpClient>(cancellationToken);

            // Get all users and filter by email/UPN
            PagedGraphUsers users = await graphClient.ListUsersAsync(cancellationToken: cancellationToken);
            GraphUser? user = users.GraphUsers.FirstOrDefault(u =>
                u.PrincipalName?.Equals(email, StringComparison.OrdinalIgnoreCase) == true);

            return (user?.OriginId ?? string.Empty, user?.DisplayName ?? string.Empty); 
        }
    }
}
