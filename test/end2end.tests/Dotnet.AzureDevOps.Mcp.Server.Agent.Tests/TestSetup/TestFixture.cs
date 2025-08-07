using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Dotnet.AzureDevOps.Mcp.Server.Agent.End2EndTests.TestSetup;

public class TestFixture : WebApplicationFactory<Program>
{
    private const string OpenAiServiceId = "openai";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        IConfiguration configuration = TestConfiguration.Configuration;

        builder
            .ConfigureLogging(logging => logging.AddConsole())
            .ConfigureAppConfiguration((_, b) => b.AddConfiguration(TestConfiguration.Configuration))
            .ConfigureTestServices(services => services
                .AddSingleton(McpConfiguration.FromConfiguration(configuration))
                .AddSingleton(AzureDevOpsConfiguration.FromConfiguration(configuration))
                .AddScoped(provider =>
                {
                    IKernelBuilder builder = Kernel.CreateBuilder();
                    McpConfiguration mcpConfiguration = provider.GetRequiredService<McpConfiguration>();

                    if(mcpConfiguration.UseAzureOpenAi)
                    {
                        builder.AddAzureOpenAIChatCompletion(
                            serviceId: OpenAiServiceId,
                            deploymentName: mcpConfiguration.AzureOpenAiDeployment,
                            endpoint: mcpConfiguration.AzureOpenAiEndpoint,
                            apiKey: mcpConfiguration.AzureOpenAiKey);
                    }
                    else if(string.IsNullOrEmpty(mcpConfiguration.SelfHostedUrl))
                    {
                        builder.AddOpenAIChatCompletion(
                            serviceId: OpenAiServiceId,
                            modelId: mcpConfiguration.OpenAiModel,
                            apiKey: mcpConfiguration.OpenAiApiKey);
                    }
                    else
                    {
                        builder.AddOpenAIChatCompletion(
                            modelId: mcpConfiguration.OpenAiModel,
                            endpoint: new Uri(mcpConfiguration.SelfHostedUrl),
                            apiKey: mcpConfiguration.OpenAiApiKey);
                    }

                    Kernel kernel = builder
                        .Build();

                    return kernel;
                }));
    }
}