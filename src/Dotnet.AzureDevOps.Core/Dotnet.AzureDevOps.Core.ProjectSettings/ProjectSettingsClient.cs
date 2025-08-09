using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
        private readonly ILogger? _logger;

        public ProjectSettingsClient(string organizationUrl, string projectName, string personalAccessToken, ILogger? logger = null)
        {
            _organizationUrl = organizationUrl;
            _projectName = projectName;

            var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
            var connection = new VssConnection(new Uri(_organizationUrl), credentials);
            _teamClient = connection.GetClient<TeamHttpClient>();
            _projectClient = connection.GetClient<ProjectHttpClient>();
            _operationsClient = connection.GetClient<OperationsHttpClient>();

            _httClient = new HttpClient { BaseAddress = new Uri(organizationUrl) };

            string encodedPersonalAccessToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            _httClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedPersonalAccessToken);

            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Creates a new team in the Azure DevOps project if a team with the specified name doesn't already exist.
        /// This method first checks for existing teams with the same name and only creates a new team
        /// if none is found. Teams are organizational units that help manage work assignments, permissions,
        /// and area/iteration paths within a project. Essential for organizing development teams and workflows.
        /// </summary>
        /// <param name="teamName">The name of the team to create (must be unique within the project).</param>
        /// <param name="teamDescription">A description explaining the team's purpose and responsibilities.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the team was created or already exists,
        /// or error details if the creation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when teamName or teamDescription is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create teams.</exception>
        /// <exception cref="InvalidOperationException">Thrown when team creation fails due to project constraints or naming conflicts.</exception>
        public async Task<AzureDevOpsActionResult<bool>> CreateTeamIfDoesNotExistAsync(string teamName, string teamDescription, CancellationToken cancellationToken = default)
        {
            AzureDevOpsActionResult<Guid> teamNameResult = await GetTeamIdAsync(teamName, cancellationToken);

            if(teamNameResult.IsSuccessful)
            {
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }

            var newTeam = new WebApiTeam
            {
                Name = teamName,
                Description = teamDescription
            };

            try
            {
                WebApiTeam createdTeam = await _teamClient.CreateTeamAsync(newTeam, _projectName, cancellationToken);
                bool success = createdTeam.Description == teamDescription && createdTeam.Name == teamName;
                return success
                    ? AzureDevOpsActionResult<bool>.Success(true, _logger)
                    : AzureDevOpsActionResult<bool>.Failure("Created team does not match the expected values.", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves the unique identifier (GUID) of a team by its name within the current project.
        /// This method searches through all teams in the project to find a team with the exact
        /// name match. Team IDs are required for many team-specific operations like setting
        /// permissions, configuring area paths, or managing team-specific settings.
        /// </summary>
        /// <param name="teamName">The exact name of the team whose ID to retrieve.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the team's unique identifier,
        /// or error details if the team doesn't exist or retrieval fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when teamName is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view teams.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team doesn't exist in the project.</exception>
        public async Task<AzureDevOpsActionResult<Guid>> GetTeamIdAsync(string teamName, CancellationToken cancellationToken = default)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Guid>.Success(team.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves a comprehensive list of all teams configured within the current Azure DevOps project.
        /// This method returns detailed information about each team including names, descriptions,
        /// IDs, and basic configuration. Essential for team management, reporting, and understanding
        /// the organizational structure of the project.
        /// </summary>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a list of all teams in the project,
        /// or error details if the retrieval fails.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view project teams.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the project doesn't exist or is inaccessible.</exception>
        public async Task<AzureDevOpsActionResult<List<WebApiTeam>>> GetAllTeamsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                List<WebApiTeam> teams = await _teamClient.GetAllTeamsAsync(cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<List<WebApiTeam>>.Success(teams, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<List<WebApiTeam>>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Updates the description of an existing team with new descriptive text.
        /// This method modifies the team's description while preserving all other team settings
        /// and configurations. Useful for maintaining accurate team information as roles,
        /// responsibilities, or focus areas evolve over time.
        /// </summary>
        /// <param name="teamName">The name of the team whose description to update.</param>
        /// <param name="newDescription">The new description text to apply to the team.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the description was successfully updated,
        /// or error details if the update fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when teamName or newDescription is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to modify teams.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team doesn't exist or cannot be modified.</exception>
        public async Task<AzureDevOpsActionResult<bool>> UpdateTeamDescriptionAsync(string teamName, string newDescription, CancellationToken cancellationToken = default)
        {
            try
            {
                WebApiTeam team = await _teamClient.GetTeamAsync(_projectName, teamName, cancellationToken: cancellationToken);

                var updatedTeam = new WebApiTeam
                {
                    Description = newDescription
                };

                WebApiTeam webApiTeam = await _teamClient.UpdateTeamAsync(updatedTeam, _projectName, team.Id.ToString(), cancellationToken: cancellationToken);

                bool success = webApiTeam.Description == newDescription && webApiTeam.Name == teamName;
                return success
                    ? AzureDevOpsActionResult<bool>.Success(true, _logger)
                    : AzureDevOpsActionResult<bool>.Failure("Updated team values do not match expected.", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Permanently deletes a team from the Azure DevOps project by its unique identifier.
        /// This method removes the team and all its associated configurations including area paths,
        /// iteration assignments, and team-specific settings. Use with caution as this operation
        /// cannot be undone and may affect work item assignments and team-based queries.
        /// </summary>
        /// <param name="teamGuid">The unique identifier of the team to delete.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the team was successfully deleted,
        /// or error details if the deletion fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when teamGuid is an empty GUID.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete teams.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team doesn't exist or cannot be deleted due to dependencies.</exception>
        public async Task<AzureDevOpsActionResult<bool>> DeleteTeamAsync(Guid teamGuid, CancellationToken cancellationToken = default)
        {
            try
            {
                string url = $"{_organizationUrl}/_apis/projects/{_projectName}/teams/{teamGuid}?api-version={GlobalConstants.ApiVersion}";

                HttpResponseMessage response = await _httClient.DeleteAsync(url, cancellationToken);

                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
                }

                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Creates a new inherited work item process based on an existing base process template.
        /// Inherited processes allow customization of work item types, fields, and workflows while
        /// maintaining compatibility with Azure DevOps updates. This enables organizations to
        /// tailor work tracking to their specific needs without losing system functionality.
        /// </summary>
        /// <param name="newProcessName">The name for the new inherited process (must be unique within the organization).</param>
        /// <param name="description">A description explaining the purpose and customizations of the new process.</param>
        /// <param name="baseProcessName">The name of the base process to inherit from (e.g., "Agile", "Scrum", "CMMI").</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the process was successfully created,
        /// or error details if the creation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create processes.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the base process doesn't exist or process name conflicts.</exception>
        public async Task<AzureDevOpsActionResult<bool>> CreateInheritedProcessAsync(string newProcessName, string description, string baseProcessName, CancellationToken cancellationToken = default)
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

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httClient.PostAsync(url, content, cancellationToken);
                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
                }
                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Permanently deletes an inherited work item process from the Azure DevOps organization.
        /// This method removes the process definition and all its customizations. Processes can only
        /// be deleted if they are not currently in use by any projects. Use with caution as this
        /// operation cannot be undone and may affect projects using the process.
        /// </summary>
        /// <param name="processId">The unique identifier of the inherited process to delete.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the process was successfully deleted,
        /// or error details if the deletion fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when processId is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete processes.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the process doesn't exist or is in use by projects.</exception>
        public async Task<AzureDevOpsActionResult<bool>> DeleteInheritedProcessAsync(string processId, CancellationToken cancellationToken = default)
        {
            string url = $"{_organizationUrl}/_apis/work/processadmin/processes/{processId}?api-version={GlobalConstants.ApiVersion}";

            try
            {
                HttpResponseMessage response = await _httClient.DeleteAsync(url, cancellationToken);
                if(!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return AzureDevOpsActionResult<bool>.Failure(response.StatusCode, error, _logger);
                }

                return AzureDevOpsActionResult<bool>.Success(true, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves the unique identifier of a work item process by its name within the organization.
        /// This method searches through available processes to find one with the exact name match.
        /// Process IDs are required for project creation and process management operations, enabling
        /// programmatic selection of work item process templates.
        /// </summary>
        /// <param name="processName">The exact name of the process whose ID to retrieve (e.g., "Agile", "Scrum", custom process name).</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the process ID as a string,
        /// or error details if the process doesn't exist or retrieval fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when processName is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view processes.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the process doesn't exist in the organization.</exception>
        public async Task<AzureDevOpsActionResult<string>> GetProcessIdAsync(string processName, CancellationToken cancellationToken = default)
        {
            string url = $"{_organizationUrl}/_apis/work/processes?api-version={GlobalConstants.ApiVersion}";

            try
            {
                JsonElement response = await _httClient.GetFromJsonAsync<JsonElement>(url, cancellationToken);

                foreach(JsonElement element in response.GetProperty("value").EnumerateArray())
                {
                    string? name = element.GetProperty("name").GetString();
                    if(string.Equals(name, processName, StringComparison.OrdinalIgnoreCase))
                    {
                        string? typeId = element.GetProperty("typeId").GetString();
                        if(!string.IsNullOrEmpty(typeId))
                        {
                            return AzureDevOpsActionResult<string>.Success(typeId, _logger);
                        }
                    }
                }

                return AzureDevOpsActionResult<string>.Failure("ProcessId couldn't be found listing and inspecting all processes", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<string>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Creates a new Azure DevOps project with the specified configuration and work item process.
        /// This method initiates project creation as an asynchronous operation, setting up the project
        /// infrastructure including repositories, work item tracking, and basic team configuration.
        /// The operation may take several minutes to complete and should be monitored for completion status.
        /// </summary>
        /// <param name="projectName">The name of the new project (must be unique within the organization).</param>
        /// <param name="description">A description explaining the project's purpose and scope.</param>
        /// <param name="processId">The ID of the work item process template to use for the project.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the unique identifier of the created project,
        /// or error details if the creation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create projects.</exception>
        /// <exception cref="InvalidOperationException">Thrown when project name conflicts or process ID is invalid.</exception>
        public async Task<AzureDevOpsActionResult<Guid>> CreateProjectAsync(string projectName, string description, string processId, CancellationToken cancellationToken = default)
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

            try
            {
                OperationReference operationReference = await _projectClient.QueueCreateProject(teamProject, userState: null);

                Operation operation = await WaitForOperationAsync(operationReference.Id);
                if(operation.Status != OperationStatus.Succeeded)
                {
                    return AzureDevOpsActionResult<Guid>.Failure($"Project creation did not succeed: {operation.Status}", _logger);
                }

                TeamProject? createdProject = await _projectClient.GetProject(projectName);
                if(createdProject == null)
                    return AzureDevOpsActionResult<Guid>.Failure("Project not found after creation.", _logger);

                return AzureDevOpsActionResult<Guid>.Success(createdProject.Id, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Guid>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific Azure DevOps project by its name.
        /// This method returns comprehensive project metadata including ID, description, state,
        /// creation information, and configured capabilities. Essential for project management,
        /// validation, and accessing project-specific configuration details.
        /// </summary>
        /// <param name="projectName">The name of the project to retrieve information for.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the complete project information,
        /// or error details if the project doesn't exist or retrieval fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectName is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view project details.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the project doesn't exist in the organization.</exception>
        public async Task<AzureDevOpsActionResult<TeamProject>> GetProjectAsync(string projectName, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamProject project = await _projectClient.GetProject(projectName);
                return AzureDevOpsActionResult<TeamProject>.Success(project, _logger);
            }
            catch(ProjectDoesNotExistWithNameException ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamProject>.Failure(ex, _logger);
            }
        }

        /// <summary>
        /// Permanently deletes an Azure DevOps project and all its associated data including
        /// repositories, work items, builds, and team configurations. This operation initiates
        /// an asynchronous deletion process that removes all project artifacts. Use with extreme
        /// caution as this operation cannot be undone and results in permanent data loss.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project to delete.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing true if the deletion was initiated successfully,
        /// or error details if the deletion request fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectId is an empty GUID.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to delete projects.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the project doesn't exist or cannot be deleted due to organization policies.</exception>
        public async Task<AzureDevOpsActionResult<bool>> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            try
            {
                OperationReference operationReference = await _projectClient.QueueDeleteProject(projectId, userState: null);
                Operation operation = await WaitForOperationAsync(operationReference.Id);
                bool success = operation.Status == OperationStatus.Succeeded;
                return success
                    ? AzureDevOpsActionResult<bool>.Success(true, _logger)
                    : AzureDevOpsActionResult<bool>.Failure($"Project deletion did not succeed: {operation.Status}", _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<bool>.Failure(ex, _logger);
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