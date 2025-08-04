using Dotnet.AzureDevOps.Core.Boards.Options;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public Task<List<BoardReference>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBoardsAsync(teamContext, userState, cancellationToken);

        public Task<TeamSettingsIteration> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationAsync(teamContext, iterationId, userState, cancellationToken);

        public Task<List<TeamSettingsIteration>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationsAsync(teamContext, timeframe, userState, cancellationToken);

        public Task<List<BoardColumn>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBoardColumnsAsync(teamContext, board.ToString(), userState, cancellationToken: cancellationToken);

        public Task<List<BacklogLevelConfiguration>> ListBacklogsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBacklogsAsync(teamContext, userState, cancellationToken);

        public Task<BacklogLevelWorkItems> ListBacklogWorkItemsAsync(TeamContext teamContext, string backlogId, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetBacklogLevelWorkItemsAsync(teamContext, backlogId, userState, cancellationToken);

        public Task<PredefinedQuery> ListMyWorkItemsAsync(string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetPredefinedQueryResultsAsync(_projectName, queryType, top, includeCompleted, userState, cancellationToken);

        public Task<IterationWorkItems> GetWorkItemsForIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetIterationWorkItemsAsync(teamContext, iterationId, userState, cancellationToken);

        public Task<List<TeamSettingsIteration>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default)
            => _workClient.GetTeamIterationsAsync(teamContext, timeFrame, userState, cancellationToken: cancellationToken);

        public async Task<IReadOnlyList<WorkItemClassificationNode>> CreateIterationsAsync(string projectName, IEnumerable<IterationCreateOptions> iterations, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(projectName);
            ArgumentNullException.ThrowIfNull(iterations);

            var created = new List<WorkItemClassificationNode>();

            foreach(IterationCreateOptions iteration in iterations)
            {
                var node = new WorkItemClassificationNode
                {
                    Name = iteration.IterationName,
                    Attributes = new Dictionary<string, object?>()
                };

                if(iteration.StartDate.HasValue)
                    node.Attributes["startDate"] = iteration.StartDate.Value;
                if(iteration.FinishDate.HasValue)
                    node.Attributes["finishDate"] = iteration.FinishDate.Value;

                WorkItemClassificationNode result = await _workItemClient.CreateOrUpdateClassificationNodeAsync(
                    postedNode: node,
                    project: projectName,
                    structureGroup: TreeStructureGroup.Iterations,
                    path: null,
                    cancellationToken: cancellationToken);

                created.Add(result);
            }

            return created;
        }

        public async Task<IReadOnlyList<TeamSettingsIteration>> AssignIterationsAsync(TeamContext teamContext, IEnumerable<IterationAssignmentOptions> iterations, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(iterations);

            var assigned = new List<TeamSettingsIteration>();
            foreach(IterationAssignmentOptions iteration in iterations)
            {
                var data = new TeamSettingsIteration
                {
                    Id = iteration.Identifier,
                    Path = iteration.Path
                };

                TeamSettingsIteration result = await _workClient.PostTeamIterationAsync(data, teamContext, cancellationToken: cancellationToken);
                assigned.Add(result);
            }

            return assigned;
        }

        public Task<TeamFieldValues> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default)
            => _workClient.GetTeamFieldValuesAsync(teamContext, cancellationToken: cancellationToken);

        public Task<Board?> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default)
            => _workClient.GetBoardAsync(teamContext, boardId, cancellationToken: cancellationToken);
    }
}

