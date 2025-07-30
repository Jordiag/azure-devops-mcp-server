using Dotnet.AzureDevOps.Tests.Common;
using Dotnet.AzureDevOps.Tests.Common.Attributes;

namespace Dotnet.AzureDevOps.Search.IntegrationTests
{
    [TestType(TestType.Integration)]
    [Component(Component.Overview)]
    public class DotnetAzureDevOpsSearchIntegrationTests : IAsyncLifetime
    {
        private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;
        private readonly List<Guid> _createdWikis = [];

        public DotnetAzureDevOpsSearchIntegrationTests()
        {
            _azureDevOpsConfiguration = AzureDevOpsConfiguration.FromEnvironment();
        }


        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {

        }

        private static string UtcStamp() =>
            DateTime.UtcNow.ToString("O").Replace(':', '-');
    }
}
