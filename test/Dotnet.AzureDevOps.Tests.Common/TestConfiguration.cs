using Microsoft.Extensions.Configuration;

namespace Dotnet.AzureDevOps.Tests.Common;

public static class TestConfiguration
{
    public static IConfiguration Configuration
        => new ConfigurationManager()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
}