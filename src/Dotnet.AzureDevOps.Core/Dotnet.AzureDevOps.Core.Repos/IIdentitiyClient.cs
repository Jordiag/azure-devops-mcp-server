namespace Dotnet.AzureDevOps.Core.Repos
{
    public interface IIdentitiyClient
    {
        Task<(string localId, string displayName)> GetUserLocalIdFromEmailAsync(
            string email,
            CancellationToken cancellationToken = default);
    }
}