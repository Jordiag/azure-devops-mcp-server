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
                .AddScoped<Kernel>(provider =>
                {
                    IKernelBuilder builder = Kernel.CreateBuilder();
                    McpConfiguration mcpConfig = provider.GetRequiredService<McpConfiguration>();

                    if(mcpConfig.UseAzureOpenAi)
                    {
                        builder.AddAzureOpenAIChatCompletion(
                            serviceId: OpenAiServiceId,
                            deploymentName: mcpConfig.AzureOpenAiDeployment,
                            endpoint: mcpConfig.AzureOpenAiEndpoint,
                            apiKey: mcpConfig.AzureOpenAiKey);
                    }
                    else if(string.IsNullOrEmpty(mcpConfig.SelfHostedUrl))
                    {
                        builder.AddOpenAIChatCompletion(
                            serviceId: OpenAiServiceId,
                            modelId: mcpConfig.OpenAiModel,
                            apiKey: mcpConfig.OpenAiApiKey);
                    }
                    else
                    {
                        builder.AddOpenAIChatCompletion(
                            modelId: mcpConfig.OpenAiModel,
                            endpoint: new Uri(mcpConfig.SelfHostedUrl),
                            apiKey: mcpConfig.OpenAiApiKey);
                    }

                    Kernel kernel = builder
                        .Build();

                    return kernel;
                }));
    }
}