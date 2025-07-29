using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

[TraitDiscoverer("Dotnet.AzureDevOps.Tests.Common.Attributes.TestTypeTraitDiscoverer", "Dotnet.AzureDevOps.Tests.Common")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestTypeAttribute : Attribute, ITraitAttribute
{
    public TestTypeAttribute(TestType type) => Type = type;

    public TestType Type { get; }
}

public enum TestType
{
    Integration,
    End2End,
    Unit
}

public sealed class TestTypeTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var type = (TestType)traitAttribute.GetConstructorArguments().First();
        yield return new KeyValuePair<string, string>("TestType", type.ToString().ToLowerInvariant());
    }
}
