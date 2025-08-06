#!/bin/bash

# Build and run Azure DevOps MCP Server in Docker
echo "Building Azure DevOps MCP Server Docker image..."

# Build the Docker image
docker build -t azure-devops-mcp-server:latest .

if [ $? -eq 0 ]; then
    echo "Docker build successful. Starting container..."
    
    # Stop existing container if running
    docker stop azure-devops-mcp-server 2>/dev/null || true
    docker rm azure-devops-mcp-server 2>/dev/null || true
    
    # Run the container
    docker run -d \
        --name azure-devops-mcp-server \
        -p 5050:5050 \
        -e ASPNETCORE_ENVIRONMENT=Production \
        -e MCP_McpServer__LogLevel=Information \
        --restart unless-stopped \
        azure-devops-mcp-server:latest
    
    echo "Container started. Server available at http://localhost:5050"
    echo "Health check: http://localhost:5050/health"
    
    # Show container logs
    echo "Container logs:"
    docker logs -f azure-devops-mcp-server
else
    echo "Docker build failed!"
    exit 1
fi
