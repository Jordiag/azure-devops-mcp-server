using Xunit.v3;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

/// <summary>
/// Custom trait attribute for categorizing tests by type (Unit, Integration, etc.).
/// In xUnit v3, ITraitAttribute requires implementing GetTraits() directly.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestTypeAttribute(TestType type) : Attribute, ITraitAttribute
{
    public TestType Type { get; } = type;

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        return [new KeyValuePair<string, string>("TestType", Type.ToString().ToLowerInvariant())];
    }
}