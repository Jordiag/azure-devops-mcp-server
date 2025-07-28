using Microsoft.Extensions.Configuration;

namespace Dotnet.AzureDevOps.Tests.Common
{
    public class McpConfiguration
    {
        public string McpServerUrl { get; private set; } = null!;
        public string OpenAiApiKey { get; private set; } = null!;
        public string OpenAiModel { get; private set; } = null!;
        public string? SelfHostedUrl { get; private set; }
        public string AzureOpenAiEndpoint { get; private set; } = null!;
        public string AzureOpenAiDeployment { get; private set; } = null!;
        public string AzureOpenAiKey { get; private set; } = null!;
        public bool UseAzureOpenAi { get; private set; } = false;

        private McpConfiguration() { }

        public static McpConfiguration FromEnvironment(IConfiguration config)
            => FromConfiguration(TestConfiguration.Configuration);

        public static McpConfiguration FromConfiguration(IConfiguration config)
            => new()
            {
                McpServerUrl = config.GetRequiredSection("MCP_SERVER_URL").Value!,
                OpenAiApiKey = config.GetRequiredSection("OPENAI_API_KEY").Value!,
                OpenAiModel = config.GetRequiredSection("OPENAI_MODEL").Value!,
                SelfHostedUrl = config.GetSection("OPENAI_SELF_HOSTED_URL").Value,
                UseAzureOpenAi = config.GetRequiredSection("USE_AZURE_OPENAI").Get<bool>()!,
                AzureOpenAiEndpoint = config.GetRequiredSection("AZURE_OPENAI_ENDPOINT").Value!,
                AzureOpenAiDeployment = config.GetRequiredSection("AZURE_OPENAI_DEPLOYMENT").Value!,
                AzureOpenAiKey = config.GetRequiredSection("AZURE_OPENAI_KEY").Value!
            };
    }
}
