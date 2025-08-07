<img src="https://github.com/user-attachments/assets/bd900648-5763-454d-b29f-fb41bab05d02" alt="azure_devops_mcp_server_logo" width="200"/>  

# Azure DevOps MCP Server for .NET

This repository contains a set of .NET libraries and a Model Context Protocol (MCP) server that expose Azure DevOps operations. The goal is to make Azure DevOps automation accessible to AI agents by surfacing common tasks—creating work items, managing pull requests, queuing builds, working with artifacts, and more—through a uniform MCP endpoint.

## Status

Currently in pre-release stage.

> **Preview Notice**

This repository is being released as a public preview to gather feedback from the community. The API surface and overall structure are still taking shape and may change substantially as development continues. Future updates might introduce breaking changes without prior notice, and functionality available today could be refactored or removed.

If you choose to build on top of this project during the preview phase, be prepared to regularly sync with the repository and adjust your code to accommodate these changes.

| Build | Integration Tests | End to End |
|-------|-------------------|------------|
|  [<img src="https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/build.yml/badge.svg" alt="Build on main" />](https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/build.yml)| [<img src="https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/integration-tests.yml/badge.svg" alt="PR integration tests" />](https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/integration-tests.yml) |[![End2End Tests](https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/end2end.yml/badge.svg)](https://github.com/Jordiag/azure-devops-mcp-server/actions/workflows/end2end.yml) |

| SonarCloud static analysis |
|------------------------------------|
|[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server)[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server)[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server)[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server)[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Jordiag_azure-devops-mcp-server&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Jordiag_azure-devops-mcp-server)|


## Overview

The repository contains multiple C# projects that wrap the Microsoft Azure DevOps SDK and REST APIs. Each Azure DevOps tab—Boards, Repos, Pipelines, Artifacts and others—has a project under `/src/` exposing a simplified client interface. These thin wrappers are consumed by `Dotnet.AzureDevOps.Mcp.Server` to surface Model Context Protocol (MCP) tools. While most calls forward to the official SDKs or, when necessary, the REST endpoints, this layer keeps the MCP server decoupled from Azure DevOps so it can evolve independently or swap implementations in the future.
The solution is organized as a multi‑project workspace targeting **.NET 9**. Each service area of Azure DevOps has its own client library:

* <img width="30px" align="center" alt="Azure Devops Overview" src="https://cdn.vsassets.io/ext/ms.vss-tfs-web/platform-content/Nav-Dashboard.S24hPD.png"/> **Overview** – manage wikis and pages.
  - Create, read, list and delete wikis
  - Create or update pages, list pages and fetch page text
* <img width="30px" align="center" alt="Azure Devops Boards" src="https://cdn.vsassets.io/ext/ms.vss-work-web/common-content/Content/Nav-Plan.XB8qU6.png"/> **Boards** – manage work items and boards.
  - Create and update Epics, Features, Stories and Tasks
  - Query work items, manage comments, attachments and links
  - Bulk updates and exports, list boards, columns and iterations
  - Manage iterations and areas and get work item counts
* <img width="30px" align="center" alt="Azure Devops Repos" src="https://cdn.vsassets.io/ext/ms.vss-code-web/common-content/Nav-Code.0tJczm.png"/> **Repos** – pull request and repository management.
  - Create, complete and list pull requests with labels and comments
  - Create tags, list branches and diffs, create and delete repositories
  - Update pull request iterations and search commits
* <img width="30px" align="center" alt="Azure Devops Pipelines" src="https://cdn.vsassets.io/ext/ms.vss-build-web/common-library/Nav-Launch.3tiJhd.png"/> **Pipelines** – build and pipeline operations.
  - Queue, cancel and retry builds, list runs and download logs
  - Retrieve changes, logs and build reports
  - List definitions and full pipeline CRUD
* <img width="30px" align="center" alt="Azure Devops Artifacts" src="https://ms.gallerycdn.vsassets.io/extensions/ms/azure-artifacts/20.258.0.1723809258/1750881068685/root/img/artifacts-icon.png"/> **Artifacts** – manage feeds and packages.
  - Create, update, list and delete feeds
  - List packages, view permissions and retention policies
  - Manage feed views and attempt package and upstreaming operations
* <img width="30px" align="center" alt="Azure Devops Test Plans" src="https://cdn.vsassets.io/ext/ms.vss-test-web/common-content/Nav-Test.CLbC8L.png"/> **Test Plans** – work with test plans and suites.
  - Create, read, list and delete test plans and suites
  - Create test cases, add them to suites and fetch test results
* <img width="30px" align="center" alt="Azure Devops Project Settings" src="https://chanlabs.com/img/2965279.png"/> **Project Settings** – team and process configuration.
  - Create, update and delete teams
  - Retrieve board settings and iterations
  - Create and delete inherited processes
* <img width="30px" align="center" alt="Azure Devops Search" src="https://chanlabs.com/img/861627.png"/> **Search** – search Azure DevOps artifacts.
  - Search wiki pages
  - Search work items

The `Dotnet.AzureDevOps.Mcp.Server` project brings these libraries together and exposes them as MCP tools. The server is implemented as a console application that hosts an ASP.NET Core web server using the [`ModelContextProtocol`](https://github.com/modelcontextprotocol) package. You can run it directly or adapt it to your preferred hosting environment—such as a container image, Azure Functions, or a Windows service. AI assistants can discover available tools at runtime and invoke them using structured function calls.

Integration tests exercise each client against a real Azure DevOps organization. Another test suite validates end‑to‑end agent interactions using [Semantic Kernel](https://github.com/microsoft/semantic-kernel), demonstrating that an LLM can automatically invoke the published tools.


## Repository structure

- **src** – source projects implementing the client libraries and MCP server.
- **test** – unit, integration and end-to-end tests.
- `Dotnet.AzureDevOps.sln` – solution file linking all projects.

## Getting started

### Prerequisites

1. Install the latest [.NET 9](https://dotnet.microsoft.com/) release.
2. Clone this repository.

### Quick Start

#### Option A: Run Locally
```bash
# Build and run
dotnet build
dotnet run --project src/Dotnet.AzureDevOps.Mcp.Server
```

#### Option B: Run with Docker
```bash
# Build and run in container
docker build -t azure-devops-mcp-server .
docker run -p 5050:5050 azure-devops-mcp-server
```

### Development Setup

For development and testing:

1. Restore dependencies and build:
   ```bash
   dotnet build
   ```
2. Run the tests:
   ```bash
   dotnet test
   ```
3. Start the MCP server:
   ```bash
   dotnet run --project src/Dotnet.AzureDevOps.Mcp.Server
   ```

The server listens on [http://localhost:5050](http://localhost:5050) by default.
Visit `/health` to check that it is running.

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

## Deployment Options

The Azure DevOps MCP Server supports two deployment methods:

### Option 1: Local HTTP Server

Run the server directly on your machine for development and testing:

```bash
# Build and run directly
dotnet run --project src/Dotnet.AzureDevOps.Mcp.Server

# Or use the provided scripts
./run-local.sh    # Linux/macOS
run-local.cmd     # Windows
```

The server listens on [http://localhost:5050](http://localhost:5050) by default.
Visit `/health` to check that it is running.

### Option 2: Docker Container

Run the server in a Docker container for consistent, isolated deployment:

```bash
# Build the Docker image
docker build -t azure-devops-mcp-server .

# Run the container
docker run -d \
    --name azure-devops-mcp-server \
    -p 5050:5050 \
    -e ASPNETCORE_ENVIRONMENT=Production \
    azure-devops-mcp-server

# Or use the provided scripts
./run-docker.sh    # Linux/macOS
run-docker.cmd     # Windows
```

The containerized server is available at [http://localhost:5050](http://localhost:5050).

#### Docker Benefits

- **Isolation**: Runs in its own environment
- **Consistency**: Same behavior across different machines
- **Production-ready**: Includes health checks and proper logging
- **Easy deployment**: Single container to deploy anywhere

### Configuration

Both deployment methods support the same configuration options:

Settings are read from `appsettings.json` and environment variables prefixed with `MCP_`.

#### Environment Variables

- `MCP_McpServer__Port=5050` - Server port (default: 5050)
- `MCP_McpServer__LogLevel=Information` - Log level
- `MCP_McpServer__EnableOpenTelemetry=true` - Enable telemetry
- `MCP_McpServer__EnableApplicationInsights=false` - Enable App Insights
- `ASPNETCORE_ENVIRONMENT=Production` - ASP.NET Core environment

#### Azure DevOps Configuration

For connecting to Azure DevOps services, set these environment variables:

- `AZURE_DEVOPS_ORG` - Your Azure DevOps organization name (e.g., "mycompany")
- `AZURE_DEVOPS_PAT` - Personal Access Token with appropriate permissions
- `AZURE_DEVOPS_SEARCH_ENDPOINT` (optional) - Custom search endpoint URL

**Example:**
```bash
export AZURE_DEVOPS_ORG=mycompany
export AZURE_DEVOPS_PAT=your_personal_access_token_here

# Optional: Custom search endpoint
export AZURE_DEVOPS_SEARCH_ENDPOINT=https://custom-search.mydomain.com/
```

#### Examples

**Local server with custom port:**
```bash
MCP_McpServer__Port=7070 dotnet run --project src/Dotnet.AzureDevOps.Mcp.Server
```

**Docker container with custom configuration:**
```bash
docker run -d \
    --name azure-devops-mcp-server \
    -p 7070:7070 \
    -e MCP_McpServer__Port=7070 \
    -e MCP_McpServer__LogLevel=Debug \
    -e MCP_McpServer__EnableOpenTelemetry=false \
    azure-devops-mcp-server
```

The script [`Set-Local-Test-Dev-Env-Vars.ps1`](Set-Local-Test-Dev-Env-Vars.ps1) can help define the environment variables used by the tests and examples.
## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)  
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.  
