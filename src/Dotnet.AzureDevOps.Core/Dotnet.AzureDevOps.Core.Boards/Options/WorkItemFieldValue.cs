namespace Dotnet.AzureDevOps.Core.Boards.Options;

/// <summary>
/// Represents a field value when creating a work item.
/// </summary>
public record WorkItemFieldValue
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Format { get; init; }
}
