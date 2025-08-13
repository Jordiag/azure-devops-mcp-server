# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Create a non-root user for security with consistent UID/GID for Kubernetes
RUN adduser --disabled-password --gecos "" --home /app --shell /bin/bash --uid 1001 appuser && \
    chown -R appuser:appuser /app

WORKDIR /app
EXPOSE 5050

# Use the .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["Dotnet.AzureDevOps.sln", "."]
COPY ["src/Dotnet.AzureDevOps.Mcp.Server/Dotnet.AzureDevOps.Mcp.Server.csproj", "src/Dotnet.AzureDevOps.Mcp.Server/"]
COPY ["src/Dotnet.AzureDevOps.Core/", "src/Dotnet.AzureDevOps.Core/"]
COPY ["Directory.Packages.props", "."]

# Restore dependencies
RUN dotnet restore "src/Dotnet.AzureDevOps.Mcp.Server/Dotnet.AzureDevOps.Mcp.Server.csproj"

# Copy only the source code needed for build (excluding sensitive files)
COPY ["src/", "src/"]

# Build the application
WORKDIR "/src/src/Dotnet.AzureDevOps.Mcp.Server"
RUN dotnet build "Dotnet.AzureDevOps.Mcp.Server.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Dotnet.AzureDevOps.Mcp.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ensure all files have proper ownership and permissions for the non-root user
USER root
RUN chown -R 1001:1001 /app && \
    chmod -R 755 /app && \
    chmod +x /app/Dotnet.AzureDevOps.Mcp.Server.dll

# Set environment variables for container
ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to non-root user for security
USER 1001

ENTRYPOINT ["dotnet", "Dotnet.AzureDevOps.Mcp.Server.dll"]
