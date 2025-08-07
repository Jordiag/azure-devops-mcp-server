using Dotnet.AzureDevOps.Core.Search;

Console.WriteLine("Azure DevOps SearchClient Environment Variable Demo");
Console.WriteLine("===================================================");

// Demonstrate error when environment variables are not set
Console.WriteLine("\n1. Testing without environment variables:");
try
{
    var client1 = SearchClient.FromEnvironment();
    Console.WriteLine("✅ Client created successfully");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Expected error: {ex.Message}");
}

// Set environment variables
Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", "mycompany");
Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", "fake-token-for-demo");

Console.WriteLine("\n2. Testing with environment variables set:");
Console.WriteLine("   AZURE_DEVOPS_ORG = mycompany");
Console.WriteLine("   AZURE_DEVOPS_PAT = fake-token-for-demo");

try
{
    var client2 = SearchClient.FromEnvironment();
    Console.WriteLine("✅ SearchClient created successfully from environment variables!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
}

// Test with custom endpoint
Environment.SetEnvironmentVariable("AZURE_DEVOPS_SEARCH_ENDPOINT", "https://custom-search.example.com/");

Console.WriteLine("\n3. Testing with custom search endpoint:");
Console.WriteLine("   AZURE_DEVOPS_SEARCH_ENDPOINT = https://custom-search.example.com/");

try
{
    var client3 = SearchClient.FromEnvironment();
    Console.WriteLine("✅ SearchClient created with custom endpoint!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
}

// Compare with traditional constructor
Console.WriteLine("\n4. Comparison with traditional constructor:");
var client4 = new SearchClient("mycompany", "fake-token-for-demo");
Console.WriteLine("✅ Traditional SearchClient constructor still works");

Console.WriteLine("\n5. Usage in Dependency Injection:");
Console.WriteLine("   The MCP server now automatically tries environment variables first,");
Console.WriteLine("   then falls back to configuration-based approach if needed.");

// Cleanup
Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", null);
Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", null);
Environment.SetEnvironmentVariable("AZURE_DEVOPS_SEARCH_ENDPOINT", null);

Console.WriteLine("\n✅ Demo completed successfully!");
