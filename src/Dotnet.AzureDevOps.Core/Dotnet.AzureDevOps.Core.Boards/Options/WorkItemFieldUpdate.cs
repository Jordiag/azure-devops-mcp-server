namespace Dotnet.AzureDevOps.Core.Boards.Options
{
    using Microsoft.VisualStudio.Services.WebApi.Patch;

    /// <summary>
    /// Describes a single update operation for a work item field.
    /// </summary>
    public record WorkItemFieldUpdate
    {
        public Operation Operation { get; init; }
        public string Path { get; init; } = string.Empty;
        public string? Value { get; init; }
        public string? Format { get; init; }
    }
}
