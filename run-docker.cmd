@echo off
REM Build and run Azure DevOps MCP Server in Docker

echo Building Azure DevOps MCP Server Docker image...

REM Build the Docker image
docker build -t azure-devops-mcp-server:latest .

if %ERRORLEVEL% equ 0 (
    echo Docker build successful. Starting container...
    
    REM Stop existing container if running
    docker stop azure-devops-mcp-server 2>nul
    docker rm azure-devops-mcp-server 2>nul
    
    REM Run the container
    docker run -d ^
        --name azure-devops-mcp-server ^
        -p 5050:5050 ^
        -e ASPNETCORE_ENVIRONMENT=Production ^
        -e MCP_McpServer__LogLevel=Information ^
        --restart unless-stopped ^
        azure-devops-mcp-server:latest
    
    echo Container started. Server available at http://localhost:5050
    echo Health check: http://localhost:5050/health
    
    REM Show initial container logs (without follow)
    echo Initial container logs:
    docker logs azure-devops-mcp-server
    
    echo.
    echo Container is running in the background.
    echo To view live logs, run: docker logs -f azure-devops-mcp-server
    echo To stop the container, run: docker stop azure-devops-mcp-server
) else (
    echo Docker build failed!
    exit /b 1
)
