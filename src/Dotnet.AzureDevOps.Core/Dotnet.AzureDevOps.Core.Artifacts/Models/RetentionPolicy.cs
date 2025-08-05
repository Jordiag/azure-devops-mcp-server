namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public class RetentionPolicy
{
    public int DaysToKeep { get; set; }
    public bool DeleteUnreferenced { get; set; }
    public bool ApplyToAllVersions { get; set; }
    public string[] PackageTypes { get; set; } = [string.Empty];
    public object[] Filters { get; set; } = [string.Empty];
}
