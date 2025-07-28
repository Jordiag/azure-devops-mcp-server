using System.Diagnostics.CodeAnalysis;
using Dotnet.AzureDevOps.Mcp.Server.Agent.Tests.TestSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;
using Xunit;

namespace Dotnet.AzureDevOps.Mcp.Server.Agent.Tests;

public sealed class McpAgentIntegrationTests : IClassFixture<TestFixture>
{
    private readonly Kernel _kernel;

    private const string EchoToolName = "echo";
    private const string EchoMessage = "Hello MCP!";

    public McpAgentIntegrationTests(TestFixture fixture)
    {
        using IServiceScope scope = fixture.Services.CreateScope();
        _kernel = scope.ServiceProvider.GetRequiredService<Kernel>();

        HttpClient client = fixture.CreateClient();
        _kernel.Plugins.AddMcpFunctionsFromSseServerAsync("MyMcpServer", client.BaseAddress!, httpClient: client)
            .GetAwaiter()
            .GetResult();

    }

    [SkippableFact(DisplayName = "Server exposes at least one MCP tool")]
    public async Task Server_ShouldExpose_Tools()
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        FunctionResult result = await _kernel.InvokePromptAsync(
            "list all the functionality actions that internally allows you to interact with azure devops", new(settings));

        string text = result.ToString() ?? string.Empty;

        Assert.False(string.IsNullOrWhiteSpace(text), "No tools returned or call failed.");
        Assert.Contains("epic", text, StringComparison.InvariantCultureIgnoreCase);
    }

    [SkippableTheory(DisplayName = "LLM calls ‘echo’ tool via function‑calling")]
    [InlineData(EchoToolName, EchoMessage)]
    public async Task Llm_ShouldInvoke_EchoTool(string toolName, string message)
    {
        string prompt = $"Call the {toolName} tool with the text \"{message}\" and return the raw output.";

        string response = string.Empty;

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var agent = new ChatCompletionAgent
        {
            Name = "McpTester",
            Kernel = _kernel,
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

    [SkippableFact(DisplayName = "Creates_WorkItem")]
    public async Task Server_ShouldCreate_WorkItemAsync()
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
        };

        var agent = new ChatCompletionAgent
        {
            Name = "McpTester",
            Kernel = _kernel,
            Instructions = "Use available tools to answer the user's question.",
            Arguments = new KernelArguments(settings)
        };

        string prompt = "please create a User Story with the title Test and all other parameters empty.";
        
        await foreach(AgentResponseItem<ChatMessageContent> update in agent.InvokeAsync(prompt))
        { }
    }
}
