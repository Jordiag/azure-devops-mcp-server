# Tests

This folder contains the test suites for the Azure DevOps MCP Server.

- **unit.tests** – traditional unit tests for the client libraries and server components.
- **integration.tests** – exercises the clients against a real Azure DevOps organization.
- **end2end.tests** – validates agent workflows end‑to‑end using Semantic Kernel.
- **Dotnet.AzureDevOps.Tests.Common** – shared utilities and fixtures for the tests.

Run all tests from the repository root with:

```bash
dotnet test
```
