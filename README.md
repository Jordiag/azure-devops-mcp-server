This repository hosts a set of .NET libraries and a Model Context Protocol (MCP) server that expose Azure DevOps operations. The goal is to make Azure DevOps automation accessible to AI agents by surfacing common tasks—creating work items, managing pull requests, queuing builds, working with artifacts and more—through a uniform MCP endpoint.



\## Overview



The solution is organized as a multi‑project workspace targeting \*\*.NET 9\*\*. Each service area of Azure DevOps has its own client library:



\- \*\*Boards\*\* – CRUD operations for Epics, Features, User Stories and Tasks.

\- \*\*Repos\*\* – pull request workflow, reviewers, comments, labels, tags and repository management.

\- \*\*Pipelines\*\* – queue, cancel and retry runs, download logs and manage pipeline definitions.

\- \*\*Artifacts\*\* – create/update feeds and list/delete packages.

\- \*\*Test Plans\*\* – manage test plans, suites and test case assignments.

\- \*\*Overview\*\* – wiki creation and page management.



The `Dotnet.AzureDevOps.Mcp.Server` project brings these libraries together and exposes them as MCP tools. The server currently runs over STDIO using the \[`ModelContextProtocol`](https://github.com/modelcontextprotocol) package. AI assistants can discover available tools at runtime and invoke them using structured function calls.



Integration tests exercise every client against a real Azure DevOps organization. Another test suite validates end‑to‑end agent interaction using \[Semantic Kernel](https://github.com/microsoft/semantic-kernel), proving that an LLM can call the published tools automatically.



\## Building



```bash

\# restore and build the entire solution

$ dotnet build Code/Dotnet.AzureDevOps.sln

```



To run the MCP server locally:



```bash

$ dotnet run --project Code/src/Dotnet.AzureDevOps.Mcp.Server

```



The server writes JSON‑RPC messages to STDOUT and listens on STDIN. Tools are discovered automatically from the assembly (see \[`Boards.cs`](Code/src/Dotnet.AzureDevOps.Mcp.Server/Boards.cs)).



\## Testing



Unit and integration tests live under the `tests` and `integration.tests` folders. Execute all tests with:



```bash

$ dotnet test Code/Dotnet.AzureDevOps.sln

```



Some integration tests require Azure DevOps credentials and may be skipped in CI if the environment variables are not supplied.



\## Contributing



Contributions are welcome! Feel free to open issues or pull requests for new features, bug fixes or improvements to documentation.



\## License



This project is licensed under the MIT License. See \[LICENSE](LICENSE) for details.

