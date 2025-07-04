namespace Dotnet.AzureDevOps.Core.Repos.Options;

public record TagCreateOptions
{
    public string Repository { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;   // e.g. "v1.2.3"

    public string CommitSha { get; init; } = string.Empty;   // object to tag

    public string TaggerName { get; init; } = "build-bot";

    public string TaggerEmail { get; init; } = "ci@example.com";

    public string? Message { get; init; }                   // null ⇒ lightweight

    public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;
}
