using Xunit.Sdk;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

[TraitDiscoverer("Dotnet.AzureDevOps.Tests.Common.Attributes.ComponentTraitDiscoverer", "Dotnet.AzureDevOps.Tests.Common")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ComponentAttribute(params Component[] components) : Attribute, ITraitAttribute
{
    public Component[] Components { get; } = components;
}
