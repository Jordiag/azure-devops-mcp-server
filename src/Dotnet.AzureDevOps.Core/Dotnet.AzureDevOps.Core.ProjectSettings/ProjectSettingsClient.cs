using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;

namespace Dotnet.AzureDevOps.Core.ProjectSettings
{
    public class ProjectSettingsClient : IProjectSettingsClient
    {
        private readonly string _organizationUrl;
        private readonly string _projectName;
        private readonly TeamHttpClient _teamClient;
        private readonly ProjectHttpClient _projectClient;
        private readonly OperationsHttpClient _operationsClient;
        private readonly string _personalAccessToken;

        public ProjectSettingsClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;
            _personalAccessToken = personalAccessToken;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _teamClient = connection.GetClient<TeamHttpClient>();
            _projectClient = connection.GetClient<ProjectHttpClient>();
            _operationsClient = connection.GetClient<OperationsHttpClient>();
        }

        public async Task<bool> CreateTeamAsync(string teamName, string teamDescription)
        {
            var newTeam = new WebApiTeam()
            {
                Name = teamName,
                Description = teamDescription
            };

            try
            {
                WebApiTeam createdTeam = await _teamClient.CreateTeamAsync(newTeam, _projectName);
                return createdTeam.Description == teamDescription && createdTeam.Name == teamName;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<Guid> GetTeamIdAsync(string teamName)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName);
                return team.Id;
            }
            catch(Exception)
            {
                return Guid.Empty;
            }
        }

        public async Task<List<WebApiTeam>> GetAllTeamsAsync()
        {
            return await _teamClient.GetAllTeamsAsync();
        }

        public async Task<bool> UpdateTeamDescriptionAsync(string teamName, string newDescription)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName);

                var updatedTeam = new WebApiTeam()
                {
                    Description = newDescription
                };

                WebApiTeam webApiTeam = await _teamClient.UpdateTeamAsync(updatedTeam, _projectName, team.Id.ToString());

                return webApiTeam.Description == newDescription && webApiTeam.Name == teamName;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTeamAsync(Guid teamGuid)
        {
            try
            {
                using var client = new HttpClient();
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                string url = $"{_organizationUrl}/_apis/projects/{_projectName}/teams/{teamGuid}?api-version={GlobalConstants.ApiVersion}";

                HttpResponseMessage response = await client.DeleteAsync(url);

                return response.IsSuccessStatusCode;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName)
        {
            string parentProcessId = baseProcessName.ToLower() switch
            {
                "agile" => "adcc42ab-9882-485e-a3ed-7678f01f66bc",
                "scrum" => "6b724908-ef14-45cf-84f8-768b5384da45",
                "cmmi" => "27450541-8e31-4150-9947-dc59f998fc01",
                _ => throw new ArgumentException("Unsupported base process name")
            };

            string url = $"{_organizationUrl}/_apis/work/processes?api-version={GlobalConstants.ApiVersion}";

            using var client = new HttpClient();

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                name = newProcessName,
                description = description,
                parentProcessTypeId = parentProcessId
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteInheritedProcessAsync(string processId)
        {
            string url = $"{_organizationUrl}/_apis/work/processadmin/processes/{processId}?api-version={GlobalConstants.ApiVersion}";

            using var client = new HttpClient();
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            HttpResponseMessage response = await client.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }

        public async Task<string?> GetProcessIdAsync(string processName)
        {
            string url = $"{_organizationUrl}/_apis/work/processes?api-version={GlobalConstants.ApiVersion}";

            using var client = new HttpClient();
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            JsonElement response = await client.GetFromJsonAsync<JsonElement>(url);

            foreach(JsonElement element in response.GetProperty("value").EnumerateArray())
            {
                string? name = element.GetProperty("name").GetString();
                if(string.Equals(name, processName, StringComparison.OrdinalIgnoreCase))
                {
                    return element.GetProperty("typeId").GetString();
                }
            }

            return null;
        }

        public async Task<Guid?> CreateProjectAsync(string projectName, string description, string processId)
        {
            var teamProject = new TeamProject
            {
                Name = projectName,
                Description = description,
                Capabilities = new Dictionary<string, Dictionary<string, string>>
                {
                    ["versioncontrol"] = new Dictionary<string, string> { ["sourceControlType"] = "Git" },
                    ["processTemplate"] = new Dictionary<string, string> { ["templateTypeId"] = processId }
                }
            };

            OperationReference operationReference = await _projectClient.QueueCreateProject(teamProject, userState: null);

            Operation operation = await WaitForOperationAsync(operationReference.Id);
            if(operation.Status != OperationStatus.Succeeded)
            {
                return null;
            }

            TeamProject? createdProject = await _projectClient.GetProject(projectName);
            return createdProject?.Id;
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId)
        {
            OperationReference operationReference = await _projectClient.QueueDeleteProject(projectId, userState: null);
            Operation operation = await WaitForOperationAsync(operationReference.Id);
            return operation.Status == OperationStatus.Succeeded;
        }

        private async Task<Operation> WaitForOperationAsync(Guid operationId)
        {
            Operation operation;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                operation = await _operationsClient.GetOperation(operationId, userState: null);
            }
            while(operation.Status == OperationStatus.InProgress || operation.Status == OperationStatus.Queued);

            return operation;
        }
    }
}
