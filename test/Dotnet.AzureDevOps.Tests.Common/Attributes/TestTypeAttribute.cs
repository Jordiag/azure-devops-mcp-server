using Xunit.Sdk;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

[TraitDiscoverer("Dotnet.AzureDevOps.Tests.Common.Attributes.TestTypeTraitDiscoverer", "Dotnet.AzureDevOps.Tests.Common")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestTypeAttribute(TestType type) : Attribute, ITraitAttribute
{
    public TestType Type { get; } = type;
}