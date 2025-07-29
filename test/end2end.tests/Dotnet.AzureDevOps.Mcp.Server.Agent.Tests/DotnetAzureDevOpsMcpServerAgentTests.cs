using Dotnet.AzureDevOps.Mcp.Server.Agent.End2EndTests.TestSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using Xunit;
using Dotnet.AzureDevOps.Tests.Common.Attributes;

namespace Dotnet.AzureDevOps.Mcp.Server.Agent.End2EndTests;

[TestType(TestType.End2End)]
public sealed class McpAgentIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly Kernel _kernel;

    private const string EchoToolName = "echo";
    private const string EchoMessage = "Hello MCP!";

    public McpAgentIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        IServiceScope scope = fixture.Services.CreateScope();
        _kernel = scope.ServiceProvider.GetRequiredService<Kernel>();
    }

    [SkippableFact(DisplayName = "Server exposes at least one MCP tool")]
    public async Task Server_ShouldExpose_Tools()
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };

        Kernel kernel = await SutAsync(tool => tool.Name.Contains("WorkItem") || tool.Name.Contains("Epic"));
        FunctionResult result = await kernel.InvokePromptAsync(
            "list all the functionality actions that internally allows you to interact with azure devops. Structure them as json based on the tool call name", new(settings));

        string text = result.ToString()?.ToLower() ?? string.Empty;

        Assert.False(string.IsNullOrWhiteSpace(text), "No tools returned or call failed.");
        Assert.Contains("deleteworkitem", text, StringComparison.InvariantCultureIgnoreCase);
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
            Kernel = await SutAsync(),
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

    private async Task<Kernel> SutAsync(Func<McpClientTool, bool> predicate)
        => await _kernel.ForMcpAsync(_fixture.Server.BaseAddress, _fixture.CreateClient(), predicate);
    private async Task<Kernel> SutAsync()
        => await _kernel.ForMcpAsync(_fixture.Server.BaseAddress, _fixture.CreateClient(), t => t.Name == "Echo");
}
