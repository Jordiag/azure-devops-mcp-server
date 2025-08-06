#!/bin/bash

# Build and run Azure DevOps MCP Server locally
echo "Building Azure DevOps MCP Server..."

# Build the solution
dotnet build --configuration Release

if [ $? -eq 0 ]; then
    echo "Build successful. Starting server..."
    
    # Run the server locally
    dotnet run --project src/Dotnet.AzureDevOps.Mcp.Server --configuration Release
else
    echo "Build failed!"
    exit 1
fi
