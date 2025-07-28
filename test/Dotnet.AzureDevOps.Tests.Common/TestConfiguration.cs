using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Dotnet.AzureDevOps.Tests.Common;

public sealed class TestConfiguration
{
    public static IConfiguration Configuration
        => new ConfigurationManager()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false)
            .AddEnvironmentVariables()
            .Build();
}