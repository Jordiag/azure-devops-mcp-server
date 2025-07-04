# Pipeline Definitions

This directory hosts the Azure DevOps YAML pipelines used to build and test the project.

- **build.yaml** – compiles the solution and runs unit tests.
- **integration-tests.yaml** – runs the integration test suite against a hosted organization.
- **end2end-tests.yaml** – executes full agent workflows using Semantic Kernel.

The pipelines run automatically in Azure DevOps when changes are pushed to the repository.
