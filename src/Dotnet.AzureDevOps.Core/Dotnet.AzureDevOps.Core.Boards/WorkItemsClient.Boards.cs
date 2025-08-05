using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        public async Task<AzureDevOpsActionResult<IReadOnlyList<BoardReference>>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<BoardReference> boards = await _workClient.GetBoardsAsync(teamContext, userState, cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<BoardReference>>.Success(boards);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<BoardReference>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<TeamSettingsIteration>> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamSettingsIteration iteration = await _workClient.GetTeamIterationAsync(teamContext, iterationId, userState, cancellationToken);
                return AzureDevOpsActionResult<TeamSettingsIteration>.Success(iteration);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamSettingsIteration>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<TeamSettingsIteration> iterations = await _workClient.GetTeamIterationsAsync(teamContext, timeframe, userState, cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Success(iterations);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<BoardColumn> columns = await _workClient.GetBoardColumnsAsync(teamContext, board.ToString(), userState, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>.Success(columns);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>> ListBacklogsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<BacklogLevelConfiguration> backlogs = await _workClient.GetBacklogsAsync(teamContext, userState, cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>.Success(backlogs);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<BacklogLevelWorkItems>> ListBacklogWorkItemsAsync(TeamContext teamContext, string backlogId, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                BacklogLevelWorkItems items = await _workClient.GetBacklogLevelWorkItemsAsync(teamContext, backlogId, userState, cancellationToken);
                return AzureDevOpsActionResult<BacklogLevelWorkItems>.Success(items);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<BacklogLevelWorkItems>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<PredefinedQuery>> ListMyWorkItemsAsync(string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                PredefinedQuery query = await _workClient.GetPredefinedQueryResultsAsync(_projectName, queryType, top, includeCompleted, userState, cancellationToken);
                return AzureDevOpsActionResult<PredefinedQuery>.Success(query);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<PredefinedQuery>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IterationWorkItems>> GetWorkItemsForIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IterationWorkItems workItems = await _workClient.GetIterationWorkItemsAsync(teamContext, iterationId, userState, cancellationToken);
                return AzureDevOpsActionResult<IterationWorkItems>.Success(workItems);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IterationWorkItems>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<TeamSettingsIteration> iterations = await _workClient.GetTeamIterationsAsync(teamContext, timeFrame, userState, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Success(iterations);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>> CreateIterationsAsync(string projectName, IEnumerable<IterationCreateOptions> iterations, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(projectName);
                ArgumentNullException.ThrowIfNull(iterations);

                List<WorkItemClassificationNode> created = new List<WorkItemClassificationNode>();

                foreach(IterationCreateOptions iteration in iterations)
                {
                    WorkItemClassificationNode node = new WorkItemClassificationNode
                    {
                        Name = iteration.IterationName,
                        Attributes = new Dictionary<string, object?>()
                    };

                    if(iteration.StartDate.HasValue)
                    {
                        node.Attributes["startDate"] = iteration.StartDate.Value;
                    }

                    if(iteration.FinishDate.HasValue)
                    {
                        node.Attributes["finishDate"] = iteration.FinishDate.Value;
                    }

                    WorkItemClassificationNode result = await _workItemClient.CreateOrUpdateClassificationNodeAsync(
                        postedNode: node,
                        project: projectName,
                        structureGroup: TreeStructureGroup.Iterations,
                        path: null,
                        cancellationToken: cancellationToken);

                    created.Add(result);
                }

                return AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>.Success(created);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> AssignIterationsAsync(TeamContext teamContext, IEnumerable<IterationAssignmentOptions> iterations, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(iterations);

                List<TeamSettingsIteration> assigned = new List<TeamSettingsIteration>();
                foreach(IterationAssignmentOptions iteration in iterations)
                {
                    TeamSettingsIteration data = new TeamSettingsIteration
                    {
                        Id = iteration.Identifier,
                        Path = iteration.Path
                    };

                    TeamSettingsIteration result = await _workClient.PostTeamIterationAsync(data, teamContext, cancellationToken: cancellationToken);
                    assigned.Add(result);
                }

                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Success(assigned);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<TeamFieldValues>> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamFieldValues values = await _workClient.GetTeamFieldValuesAsync(teamContext, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<TeamFieldValues>.Success(values);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamFieldValues>.Failure(ex);
            }
        }

        public async Task<AzureDevOpsActionResult<Board>> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default)
        {
            try
            {
                Board board = await _workClient.GetBoardAsync(teamContext, boardId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Board>.Success(board);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Board>.Failure(ex);
            }
        }
    }
}

