namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public class FeedPermissionList
{
    public int Count { get; set; }
    public required FeedPermission[] Value { get; set; }
}

