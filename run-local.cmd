@echo off
REM Build and run Azure DevOps MCP Server locally

echo Building Azure DevOps MCP Server...

REM Build the solution
dotnet build --configuration Release

if %ERRORLEVEL% equ 0 (
    echo Build successful. Starting server...
    
    REM Run the server locally
    dotnet run --project src/Dotnet.AzureDevOps.Mcp.Server --configuration Release
) else (
    echo Build failed!
    exit /b 1
)
