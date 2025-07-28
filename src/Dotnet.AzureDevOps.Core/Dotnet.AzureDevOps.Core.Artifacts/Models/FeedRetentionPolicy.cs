namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public record FeedRetentionPolicy
{
    public int? AgeLimitInDays { get; init; }
    public int? CountLimit { get; init; }
    public int? DaysToKeepRecentlyDownloadedPackages { get; init; }
}

