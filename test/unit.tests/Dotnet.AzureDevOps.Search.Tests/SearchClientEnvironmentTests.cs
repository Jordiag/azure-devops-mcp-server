using Dotnet.AzureDevOps.Core.Search;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Dotnet.AzureDevOps.Search.Tests;

public class SearchClientEnvironmentTests
{
    [Fact]
    public void FromEnvironment_WhenVariablesNotSet_ThrowsInvalidOperationException()
    {
        // Arrange - Clear environment variables
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", null);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", null);

        // Act & Assert
        var orgException = Assert.Throws<InvalidOperationException>(
            () => SearchClient.FromEnvironment());
        
        Assert.Contains("AZURE_DEVOPS_ORG", orgException.Message);
    }

    [Fact]
    public void FromEnvironment_WhenVariablesSet_CreatesClient()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", "testorg");
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", "testpat");
        
        try
        {
            // Act
            var client = SearchClient.FromEnvironment();

            // Assert
            Assert.NotNull(client);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", null);
            Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", null);
        }
    }

    [Fact]
    public void FromEnvironment_WithCustomEndpoint_UsesCustomEndpoint()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", "testorg");
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", "testpat");
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_SEARCH_ENDPOINT", "https://custom.search.example.com/");
        
        try
        {
            // Act
            var client = SearchClient.FromEnvironment();

            // Assert
            Assert.NotNull(client);
            // Note: We can't easily test the internal HttpClient BaseAddress without making it public
            // This test mainly verifies the client can be created with custom endpoint environment variable
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", null);
            Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", null);
            Environment.SetEnvironmentVariable("AZURE_DEVOPS_SEARCH_ENDPOINT", null);
        }
    }
}
