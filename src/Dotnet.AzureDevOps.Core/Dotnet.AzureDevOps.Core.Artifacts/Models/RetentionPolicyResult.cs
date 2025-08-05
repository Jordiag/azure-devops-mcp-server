namespace Dotnet.AzureDevOps.Core.Artifacts.Models;

public class RetentionPolicyResult
{
    public Guid Id { get; set; }
    public Guid FeedId { get; set; }
    public required RetentionPolicy RetentionPolicy { get; set; }
    public required IdentityRef CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}
