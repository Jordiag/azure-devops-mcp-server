namespace Dotnet.AzureDevOps.Core.Boards.Options
{
    /// <summary>
    /// Defines all the optional/required fields for creating a work item in Azure DevOps.
    /// </summary>
    public record WorkItemCreateOptions
    {
        public string? Title { get; init; }

        public string? Description { get; init; }

        public string? AssignedTo { get; init; }

        public string? State { get; init; }

        public string? Tags { get; init; }

        public double? StoryPoints { get; init; }

        public double? Effort { get; init; }

        public double? RemainingWork { get; init; }

        public double? OriginalEstimate { get; init; }

        public int? Priority { get; init; }

        public string? AreaPath { get; init; }

        public string? IterationPath { get; init; }

        public string? AcceptanceCriteria { get; init; }

        public int? ParentId { get; init; }
    }
}
