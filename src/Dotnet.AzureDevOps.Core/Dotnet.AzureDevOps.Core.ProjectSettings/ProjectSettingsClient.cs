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
        private readonly HttpClient _httClient;
        private readonly TeamHttpClient _teamClient;
        private readonly ProjectHttpClient _projectClient;
        private readonly OperationsHttpClient _operationsClient;


        public ProjectSettingsClient(string organizationUrl, string projectName, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;

            VssBasicCredential credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            VssConnection connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _teamClient = connection.GetClient<TeamHttpClient>();
            _projectClient = connection.GetClient<ProjectHttpClient>();
            _operationsClient = connection.GetClient<OperationsHttpClient>();

            _httClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };

            string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            _httClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);
        }

        public async Task<AzureDevOpsActionResult<bool>> CreateTeamAsync(string teamName, string teamDescription)
        {
            WebApiTeam newTeam = new WebApiTeam
            {
                Name = teamName,
                Description = teamDescription
            };

            try
            {
                WebApiTeam createdTeam = await _teamClient.CreateTeamAsync(newTeam, _projectName);
                bool success = createdTeam.Description == teamDescription && createdTeam.Name == teamName;
                return success
                    ? AzureDevOpsActionResult<bool>.Success(true)
                    : AzureDevOpsActionResult<bool>.Failure("Created team does not match the expected values.");
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<Guid>> GetTeamIdAsync(string teamName)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName);
                return AzureDevOpsActionResult<Guid>.Success(team.Id);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<List<WebApiTeam>>> GetAllTeamsAsync()
        {
            try
            {
                List<WebApiTeam> teams = await _teamClient.GetAllTeamsAsync();
                return AzureDevOpsActionResult<List<WebApiTeam>>.Success(teams);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<List<WebApiTeam>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> UpdateTeamDescriptionAsync(string teamName, string newDescription)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName);

                WebApiTeam updatedTeam = new WebApiTeam
                {
                    Description = newDescription
                };

                WebApiTeam webApiTeam = await _teamClient.UpdateTeamAsync(updatedTeam, _projectName, team.Id.ToString());

                bool success = webApiTeam.Description == newDescription && webApiTeam.Name == teamName;
                return success
                    ? AzureDevOpsActionResult<bool>.Success(true)
                    : AzureDevOpsActionResult<bool>.Failure("Updated team values do not match expected.");
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteTeamAsync(Guid teamGuid)
        {
            try
            {
                string url = $"{_organizationUrl}/_apis/projects/{_projectName}/teams/{teamGuid}?api-version={GlobalConstants.ApiVersion}";

                HttpResponseMessage response = await _httClient.DeleteAsync(url);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
                }

                return AzureDevOpsActionResult<bool>.Success(true);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName)
        {
            string parentProcessId = baseProcessName.ToLower() switch
            {
                "agile" => "adcc42ab-9882-485e-a3ed-7678f01f66bc",
                "scrum" => "6b724908-ef14-45cf-84f8-768b5384da45",
                "cmmi" => "27450541-8e31-4150-9947-dc59f998fc01",
                _ => throw new ArgumentException("Unsupported base process name")
            };

            string url = $"{_organizationUrl}_apis/projects?api-version={GlobalConstants.ApiVersion}";

            object payload = new
            {
                name = newProcessName,
                description = description,
                parentProcessTypeId = parentProcessId
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httClient.PostAsync(url, content);
                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
                }
                return AzureDevOpsActionResult<bool>.Success(true);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteInheritedProcessAsync(string processId)
        {
            string url = $"{_organizationUrl}/_apis/work/processadmin/processes/{processId}?api-version={GlobalConstants.ApiVersion}";

            try
            {
                HttpResponseMessage response = await _httClient.DeleteAsync(url);
                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error);
                }

                return AzureDevOpsActionResult<bool>.Success(true);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<string>> GetProcessIdAsync(string processName)
        {
            string url = $"{_organizationUrl}/_apis/work/processes?api-version={GlobalConstants.ApiVersion}";

            try
            {
                JsonElement response = await _httClient.GetFromJsonAsync<JsonElement>(url);

                foreach(JsonElement element in response.GetProperty("value").EnumerateArray())
                {
                    string? name = element.GetProperty("name").GetString();
                    if(string.Equals(name, processName, StringComparison.OrdinalIgnoreCase))
                    {
                        string? typeId = element.GetProperty("typeId").GetString();
                        return AzureDevOpsActionResult<string>.Success(typeId);
                    }
                }

                return AzureDevOpsActionResult<string>.Success(null);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<Guid>> CreateProjectAsync(string projectName, string description, string processId)
        {
            TeamProject teamProject = new TeamProject
            {
                Name = projectName,
                Description = description,
                Capabilities = new Dictionary<string, Dictionary<string, string>>
                {
                    ["versioncontrol"] = new Dictionary<string, string> { ["sourceControlType"] = "Git" },
                    ["processTemplate"] = new Dictionary<string, string> { ["templateTypeId"] = processId }
                }
            };

            try
            {
                OperationReference operationReference = await _projectClient.QueueCreateProject(teamProject, userState: null);

                Operation operation = await WaitForOperationAsync(operationReference.Id);
                if(operation.Status != OperationStatus.Succeeded)
                {
                    return AzureDevOpsActionResult<Guid>.Failure($"Project creation did not succeed: {operation.Status}");
                }

                TeamProject? createdProject = await _projectClient.GetProject(projectName);
                if(createdProject == null)
                    return AzureDevOpsActionResult<Guid>.Failure("Project not found after creation.");

                return AzureDevOpsActionResult<Guid>.Success(createdProject.Id);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<TeamProject>> GetProjectAsync(string projectName)
        {
            try
            {
                TeamProject project = await _projectClient.GetProject(projectName);
                return AzureDevOpsActionResult<TeamProject>.Success(project);
            }
            catch(ProjectDoesNotExistWithNameException ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<bool>> DeleteProjectAsync(Guid projectId)
        {
            try
            {
                OperationReference operationReference = await _projectClient.QueueDeleteProject(projectId, userState: null);
                Operation operation = await WaitForOperationAsync(operationReference.Id);
                bool success = operation.Status == OperationStatus.Succeeded;
                return success
                    ? AzureDevOpsActionResult<bool>.Success(true)
                    : AzureDevOpsActionResult<bool>.Failure($"Project deletion did not succeed: {operation.Status}");
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex);
            }
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
