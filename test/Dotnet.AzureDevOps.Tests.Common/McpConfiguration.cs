using System.Diagnostics.CodeAnalysis;

namespace Dotnet.AzureDevOps.Tests.Common
{
    [ExcludeFromCodeCoverage]
    public class McpConfiguration
    {
        public string McpServerUrl { get; }
        public string OpenAiApiKey { get; }
        public string OpenAiModel { get; }
        public string AzureOpenAiEndpoint { get; }
        public string AzureOpenAiDeployment { get; }
        public string AzureOpenAiKey { get; }
        public bool UseAzureOpenAi { get; }

        public McpConfiguration()
        {
            McpServerUrl = GetEnv("MCP_SERVER_URL");
            OpenAiApiKey = GetEnv("OPENAI_API_KEY");
            OpenAiModel = GetEnv("OPENAI_MODEL");

            UseAzureOpenAi  = bool.TryParse(GetEnv("USE_AZURE_OPENAI"), out bool b) && b;
            AzureOpenAiEndpoint = GetEnv("AZURE_OPENAI_ENDPOINT");
            AzureOpenAiDeployment = GetEnv("AZURE_OPENAI_DEPLOYMENT");
            AzureOpenAiKey = GetEnv("AZURE_OPENAI_KEY");
        }

        private static string GetEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new ArgumentException($"{name} environment variable is missing.");
    }
}
