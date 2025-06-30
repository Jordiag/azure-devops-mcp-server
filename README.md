<img src="https://github.com/user-attachments/assets/bd900648-5763-454d-b29f-fb41bab05d02" alt="azure_devops_mcp_server_logo" width="400"/>

This repository will host a set of .NET libraries and a Model Context Protocol (MCP) server that expose Azure DevOps operations. The goal is to make Azure DevOps automation accessible to AI agents by surfacing common tasks—creating work items, managing pull requests, queuing builds, working with artifacts, and more—through a uniform MCP endpoint.

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

Integration tests exercise each client against a real Azure DevOps organization. Another test suite validates end‑to‑end agent interactions using [Semantic Kernel](https://github.com/microsoft/semantic-kernel), demonstrating that an LLM can automatically invoke the published tools.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
