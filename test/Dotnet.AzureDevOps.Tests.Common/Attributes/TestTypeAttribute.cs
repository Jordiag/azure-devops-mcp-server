using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

[TraitDiscoverer("Dotnet.AzureDevOps.Tests.Common.Attributes.TestTypeTraitDiscoverer", "Dotnet.AzureDevOps.Tests.Common")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestTypeAttribute(TestType type) : Attribute, ITraitAttribute
{
    public TestType Type { get; } = type;
}

[TraitDiscoverer("Dotnet.AzureDevOps.Tests.Common.Attributes.ComponentTraitDiscoverer", "Dotnet.AzureDevOps.Tests.Common")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ComponentAttribute(params Component[] components) : Attribute, ITraitAttribute
{
    public Component[] Components { get; } = components;
}

public enum TestType
{
    Integration,
    End2End,
    Unit
}

public enum Component
{
    Artifacts,
    Boards,
    Overview,
    Pipelines,
    ProjectSettings,
    Repos,
    TestPlans
}

public sealed class TestTypeTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var type = (TestType)traitAttribute.GetConstructorArguments().First();
        yield return new KeyValuePair<string, string>("TestType", type.ToString().ToLowerInvariant());
    }
}

public sealed class ComponentTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var components = (Component[])traitAttribute.GetConstructorArguments().First();
        foreach(Component component in components)
        {
            yield return new KeyValuePair<string, string>("Component", component.ToString().ToLowerInvariant());
        }
    }
}