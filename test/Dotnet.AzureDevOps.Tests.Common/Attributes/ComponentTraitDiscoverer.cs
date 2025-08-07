using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

public sealed class ComponentTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        Component[] components = (Component[])traitAttribute.GetConstructorArguments().First();
        foreach(Component component in components)
        {
            yield return new KeyValuePair<string, string>("Component", component.ToString().ToLowerInvariant());
        }
    }
}
