using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;

namespace Dotnet.AzureDevOps.Mcp.Server.Agent.Tests;

[ExcludeFromCodeCoverage]
public sealed class McpAgentIntegrationTests : IAsyncLifetime
{
    private Kernel? _kernel;
    private readonly McpConfiguration _mcpConfiguration;
    private readonly Uri _serverUri;
    private readonly string _openAiKey;
    private readonly string _openAiModel;

    private const string EchoToolName = "echo";
    private const string EchoMessage = "Hello MCP!";
    private const string ServiceId = "openai";

    public McpAgentIntegrationTests()
    {
        _mcpConfiguration = new McpConfiguration();
        _serverUri = new Uri(_mcpConfiguration.McpServerUrl);
        _openAiKey = _mcpConfiguration.OpenAiApiKey;
        _openAiModel = _mcpConfiguration.OpenAiModel;
    }

    public Task InitializeAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(c => c.AddConsole());

        if(_mcpConfiguration.UseAzureOpenAi)
        {
            // Azure OpenAI
            builder.Services.AddAzureOpenAIChatCompletion(
                serviceId: ServiceId,
                deploymentName: _mcpConfiguration.AzureOpenAiDeployment,
                endpoint: _mcpConfiguration.AzureOpenAiEndpoint,
                apiKey: _mcpConfiguration.AzureOpenAiKey);
        }
        else
        {
            // Public OpenAI
            builder.Services.AddOpenAIChatCompletion(
                serviceId: ServiceId,
                modelId: _openAiModel,
                apiKey: _openAiKey);
        }

        _kernel = builder.Build();

        return Task.CompletedTask;
    }

    private async Task RegisterMcpServerAsync()
    {
        try
        {
            if(_kernel == null)
            {
                throw new InvalidOperationException("Kernel is not initialized.");
            }
            await _kernel.Plugins.AddMcpFunctionsFromSseServerAsync("MyMcpServer", _serverUri.ToString());
        }
        catch(TimeoutException ex)
        {
            throw new SkipException($"Mcp server not ready, ignored: {ex.Message}");
        }
        catch(System.Net.Http.HttpRequestException ex)
        {
            throw new SkipException($"Mcp server not ready, ignored: {ex.Message}");
        }
    }

    [SkippableFact(DisplayName = "Server exposes at least one MCP tool")]
    public async Task Server_ShouldExpose_Tools()
    {
        await RegisterMcpServerAsync();

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        FunctionResult result = await _kernel!.InvokePromptAsync(
            "list all the functionality actions that internally allows you to interact with azure devops", new(settings));

        string text = result.ToString() ?? string.Empty;

        Assert.False(string.IsNullOrWhiteSpace(text), "No tools returned or call failed.");
        Assert.Contains("epic", text, StringComparison.InvariantCultureIgnoreCase);
    }

    [SkippableTheory(DisplayName = "LLM calls ‘echo’ tool via function‑calling")]
    [InlineData(EchoToolName, EchoMessage)]
    public async Task Llm_ShouldInvoke_EchoTool(string toolName, string message)
    {
        await RegisterMcpServerAsync();

        string prompt = $"Call the {toolName} tool with the text \"{message}\" and return the raw output.";

        string response = string.Empty;

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var agent = new ChatCompletionAgent
        {
            Name = "McpTester",
            Kernel = _kernel!,
            Instructions = "Use available tools to answer the user's question.",
            Arguments = new KernelArguments(settings)
        };

        await foreach(AgentResponseItem<ChatMessageContent> update in agent.InvokeAsync(prompt))
        {
            if(!string.IsNullOrWhiteSpace(update.Message?.ToString()))
                response = update.Message!.ToString();
        }

        Assert.Contains(message, response, StringComparison.OrdinalIgnoreCase);
    }

    public Task DisposeAsync() => Task.CompletedTask;

}
