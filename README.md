# Azure DevOps MCP Server for .NET

This repository will host a set of .NET libraries and a Model Context Protocol (MCP) server that expose Azure DevOps operations. The goal is to make Azure DevOps automation accessible to AI agents by surfacing common tasks—creating work items, managing pull requests, queuing builds, working with artifacts, and more—through a uniform MCP endpoint.

## Status

Currently in [pre-alpha](https://en.wikipedia.org/wiki/Software_release_life_cycle#Pre-alpha) release stage, soon will be deployed to this repo.

| Build | Integration Tests | SonarCloud static analisys | Nuget |  
|-------|------------|-----------------|-------|
|  [![Build Status](https://dev.azure.com/Chanlabs/Dotnet.AzureDevOps/_apis/build/status%2FBuild?branchName=main)](https://github.com/Jordiag/azure-devops-mcp-server)|[![Build Status](https://dev.azure.com/Chanlabs/Dotnet.AzureDevOps/_apis/build/status%2FIntegration%20Tests?branchName=main)](https://github.com/Jordiag/azure-devops-mcp-server)   |       [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps)[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps)<br> [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Chanlabs_Dotnet.AzureDevOps&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Chanlabs_Dotnet.AzureDevOps) | <img src="https://user-images.githubusercontent.com/8865104/208910828-d9a283f0-d8f4-4fc2-ac45-a8b5ac65b2e7.svg" alt="not-available" width="20" height="20" align="center" /> N/A yet  |


## Overview

The solution is organized as a multi‑project workspace targeting **.NET 9**. Each service area of Azure DevOps has its own client library:

* **Boards** – CRUD 
operations for Epics, Features, User Stories, and Tasks.
* **Repos** – Pull request workflows, reviewers, comments, labels, tags, and repository management.
* **Pipelines** – Queue, cancel, and retry runs; download logs; and manage pipeline definitions.
* **Artifacts** – Create/update feeds and list/delete packages.
* **Test Plans** – Manage test plans, suites, and test case assignments.
* **Wiki** – Wiki creation and page management.

The `Dotnet.AzureDevOps.Mcp.Server` project brings these libraries together and exposes them as MCP tools. The server currently runs over STDIO using the [`ModelContextProtocol`](https://github.com/modelcontextprotocol) package. AI assistants can discover available tools at runtime and invoke them using structured function calls.
The `Dotnet.AzureDevOps.Mcp.Server` project brings these libraries together and exposes them as MCP tools. The server is implemented as a console application that hosts an ASP.NET Core web server using the [`ModelContextProtocol`](https://github.com/modelcontextprotocol) package. You can run it directly or adapt it to your preferred hosting environment—such as a container image, Azure Functions, or a Windows service. AI assistants can discover available tools at runtime and invoke them using structured function calls.

Integration tests exercise each client against a real Azure DevOps organization. Another test suite validates end‑to‑end agent interactions using [Semantic Kernel](https://github.com/microsoft/semantic-kernel), demonstrating that an LLM can automatically invoke the published tools.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)  
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.  
