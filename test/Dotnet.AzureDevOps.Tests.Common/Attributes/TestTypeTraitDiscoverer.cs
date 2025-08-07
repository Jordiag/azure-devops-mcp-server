using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dotnet.AzureDevOps.Tests.Common.Attributes;

public sealed class TestTypeTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        TestType type = (TestType)traitAttribute.GetConstructorArguments().First();
        yield return new KeyValuePair<string, string>("TestType", type.ToString().ToLowerInvariant());
    }
}
