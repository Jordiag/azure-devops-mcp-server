using Xunit.v3;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

/// <summary>
/// Custom trait attribute for categorizing tests by component.
/// In xUnit v3, ITraitAttribute requires implementing GetTraits() directly.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ComponentAttribute(params Component[] components) : Attribute, ITraitAttribute
{
    public Component[] Components { get; } = components;

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        var traits = new List<KeyValuePair<string, string>>();
        foreach (Component component in Components)
        {
            traits.Add(new KeyValuePair<string, string>("Component", component.ToString().ToLowerInvariant()));
        }
        return traits;
    }
}
