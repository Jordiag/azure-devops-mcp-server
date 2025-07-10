<img src="https://github.com/user-attachments/assets/bd900648-5763-454d-b29f-fb41bab05d02" alt="azure_devops_mcp_server_logo" width="200"/>
# Azure DevOps MCP Server for .NET

This repository contains a set of .NET libraries and a Model Context Protocol (MCP) server that expose Azure DevOps operations. The goal is to make Azure DevOps automation accessible to AI agents by surfacing common tasks—creating work items, managing pull requests, queuing builds, working with artifacts, and more—through a uniform MCP endpoint.

## Status

Currently in pre-release stage.

> **Preview Notice**

This repository is being released as a public preview to gather feedback from the community. The API surface and overall structure are still taking shape and may change substantially as development continues. Future updates might introduce breaking changes without prior notice, and functionality available today could be refactored or removed.

If you choose to build on top of this project during the preview phase, be prepared to regularly sync with the repository and adjust your code to accommodate these changes.
| Build | Integration Tests | SonarCloud static analysis |
|-------|------------|-----------------|
|  [<img src="https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/build.yml/badge.svg" width="265" alt="Build on main" />](https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/build.yml)| [<img src="https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/integration-tests.yml/badge.svg" width="312" alt="PR integration tests" />](https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/integration-tests.yml) |  [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps)|


## Overview

The repository contains multiple C# projects that wrap the Microsoft Azure DevOps SDK and REST APIs. Each Azure DevOps tab—Boards, Repos, Pipelines, Artifacts and others—has a project under `/src/` exposing a simplified client interface. These thin wrappers are consumed by `Dotnet.AzureDevOps.Mcp.Server` to surface Model Context Protocol (MCP) tools. While most calls forward to the official SDKs or, when necessary, the REST endpoints, this layer keeps the MCP server decoupled from Azure DevOps so it can evolve independently or swap implementations in the future.
The solution is organized as a multi‑project workspace targeting **.NET 9**. Each service area of Azure DevOps has its own client library:

* **Boards** – CRUD 
operations for Epics, Features, User Stories, and Tasks.
* **Repos** – Pull request workflows, reviewers, comments, labels, tags, and repository management.
* **Pipelines** – Queue, cancel, and retry runs; download logs; and manage pipeline definitions.
* **Artifacts** – Create/update feeds and list/delete packages.
* **Test Plans** – Manage test plans, suites, and test case assignments.
* **Wiki** – Wiki creation and page management.

The `Dotnet.AzureDevOps.Mcp.Server` project brings these libraries together and exposes them as MCP tools. The server is implemented as a console application that hosts an ASP.NET Core web server using the [`ModelContextProtocol`](https://github.com/modelcontextprotocol) package. You can run it directly or adapt it to your preferred hosting environment—such as a container image, Azure Functions, or a Windows service. AI assistants can discover available tools at runtime and invoke them using structured function calls.

Integration tests exercise each client against a real Azure DevOps organization. Another test suite validates end‑to‑end agent interactions using [Semantic Kernel](https://github.com/microsoft/semantic-kernel), demonstrating that an LLM can automatically invoke the published tools.


## Repository structure

- **src** – source projects implementing the client libraries and MCP server.
- **test** – unit, integration and end-to-end tests.
- **Pipelines** – Azure DevOps YAML pipeline definitions.
- `Dotnet.AzureDevOps.sln` – solution file linking all projects.

## Getting started

1. Install the latest [.NET 9](https://dotnet.microsoft.com/) release.
2. Clone this repository.
3. Restore dependencies and build:
   ```bash
   dotnet build
   ```
4. Run the tests:
   ```bash
   dotnet test
   ```

## Example: Using the MCP server with Semantic Kernel

The [end‑to‑end tests](test/end2end.tests/Dotnet.AzureDevOps.Mcp.Server.Agent.Tests)
show how a Semantic Kernel agent can call tools published by the MCP server. A
new console application can reproduce the same workflow:

### Prerequisites

- .NET 9 SDK installed
- A running MCP server accessible via URL
- An OpenAI API key and model name
- Environment variables configured:
  - `MCP_SERVER_URL`
  - `OPENAI_API_KEY`
  - `OPENAI_MODEL`

```bash
dotnet new console -n MyMcpClient
cd MyMcpClient
dotnet add package Microsoft.SemanticKernel --version 1.59.0
dotnet add package Microsoft.SemanticKernel.Agents.Core --version 1.59.0
dotnet add package ModelContextProtocol-SemanticKernel --version 0.3.0-preview-01
```

Replace `Program.cs` with the following and configure your MCP server URL and
OpenAI credentials as environment variables:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;

var serverUrl = Environment.GetEnvironmentVariable("MCP_SERVER_URL")!;
var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
var model = Environment.GetEnvironmentVariable("OPENAI_MODEL")!;

IKernelBuilder builder = Kernel.CreateBuilder();
builder.Services.AddOpenAIChatCompletion("openai", model, openAiKey);
var kernel = builder.Build();

await kernel.Plugins.AddMcpFunctionsFromSseServerAsync("MyMcpServer", serverUrl);

var settings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var agent = new ChatCompletionAgent
{
    Name = "McpTester",
    Kernel = kernel,
    Instructions = "Use available tools to answer the user's question.",
    Arguments = new KernelArguments(settings)
};

await foreach (var update in agent.InvokeAsync(
    "Call the echo tool with the text \"Hello MCP!\" and return the raw output."))
{
    Console.WriteLine(update.Message);
}
```

Running this program prints `Hello MCP!` once the agent invokes the `echo` tool
exposed by your MCP server.
## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)  
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.  
