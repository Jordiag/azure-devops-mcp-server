using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Dotnet.AzureDevOps.Core.Boards
{
    public partial class WorkItemsClient
    {
        /// <summary>
        /// Retrieves all available boards for a specified team, providing access to Kanban and Scrum boards
        /// configured for the team. Boards represent different work item types (stories, bugs, tasks, etc.)
        /// and their workflow states. This is essential for understanding the team's board structure and
        /// available work tracking mechanisms.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of board references,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view team boards.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team or project doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<BoardReference>>> ListBoardsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<BoardReference> boards = await _workClient.GetBoardsAsync(teamContext, userState, cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<BoardReference>>.Success(boards, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<BoardReference>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific team iteration (sprint) by its unique identifier.
        /// Team iterations define the time-boxed periods during which work is completed, typically in Agile
        /// methodologies. The iteration contains start/end dates, capacity information, and associated
        /// work items, providing comprehensive sprint planning and tracking capabilities.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="iterationId">The unique identifier of the iteration to retrieve.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the team settings iteration,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown when iterationId is an empty GUID.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view team iterations.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team, project, or iteration doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<TeamSettingsIteration>> GetTeamIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamSettingsIteration iteration = await _workClient.GetTeamIterationAsync(teamContext, iterationId, userState, cancellationToken);
                return AzureDevOpsActionResult<TeamSettingsIteration>.Success(iteration, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamSettingsIteration>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves all team iterations within a specified timeframe, providing a comprehensive view
        /// of the team's sprint schedule. Timeframes can include current, past, or future iterations,
        /// enabling sprint planning, retrospectives, and progress tracking across multiple iterations.
        /// This is essential for Agile teams managing multiple sprints and planning work distribution.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="timeframe">The timeframe filter for iterations (e.g., "current", "past", "future").</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of team iterations,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown when timeframe is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view team iterations.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team or project doesn't exist, or timeframe is invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> GetTeamIterationsAsync(TeamContext teamContext, string timeframe, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<TeamSettingsIteration> iterations = await _workClient.GetTeamIterationsAsync(teamContext, timeframe, userState, cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Success(iterations, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves the column configuration for a specific board, providing information about workflow states
        /// and board structure. Board columns represent the different stages in a work item's lifecycle
        /// (such as New, Active, Resolved, Closed) and help visualize work progress through the team's
        /// workflow. Each column contains rules, limits, and state mappings that control work item movement.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="board">The unique identifier of the board whose columns to retrieve.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of board columns,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown when board is an empty GUID.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view board configuration.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team, project, or board doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>> ListBoardColumnsAsync(TeamContext teamContext, Guid board, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<BoardColumn> columns = await _workClient.GetBoardColumnsAsync(teamContext, board.ToString(), userState, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>.Success(columns, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<BoardColumn>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves the backlog level configuration for a team, providing information about the hierarchical
        /// structure of work items. Backlogs represent different levels of work organization (Epics, Features,
        /// User Stories, Tasks) and help teams manage work from high-level strategic initiatives down to
        /// detailed implementation tasks. Each backlog level has associated work item types and rules.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of backlog level configurations,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view backlog configuration.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team or project doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>> ListBacklogsAsync(TeamContext teamContext, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<BacklogLevelConfiguration> backlogs = await _workClient.GetBacklogsAsync(teamContext, userState, cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>.Success(backlogs, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<BacklogLevelConfiguration>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves all work items associated with a specific backlog level, providing a comprehensive view
        /// of work at that hierarchical level. This includes work items directly assigned to the backlog
        /// as well as their hierarchical relationships. Backlog work items help teams prioritize and plan
        /// work at different organizational levels (strategic, tactical, operational).
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="backlogId">The identifier of the backlog level to retrieve work items from.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the backlog level work items with hierarchical information,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown when backlogId is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view backlog work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team, project, or backlog doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<BacklogLevelWorkItems>> ListBacklogWorkItemsAsync(TeamContext teamContext, string backlogId, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                BacklogLevelWorkItems items = await _workClient.GetBacklogLevelWorkItemsAsync(teamContext, backlogId, userState, cancellationToken);
                return AzureDevOpsActionResult<BacklogLevelWorkItems>.Success(items, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<BacklogLevelWorkItems>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves work items assigned to the current user based on predefined query criteria.
        /// This provides a personalized view of work items, commonly used for dashboard displays
        /// and personal work tracking. Query types include work assigned to the user, work created
        /// by the user, or work that the user is following. Results can be filtered to include
        /// or exclude completed work items.
        /// </summary>
        /// <param name="queryType">The type of personal work query to execute (default: "assignedtome").</param>
        /// <param name="top">Optional limit on the number of work items to return.</param>
        /// <param name="includeCompleted">Optional flag to include completed work items in results.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the predefined query results with matching work items,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when queryType is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to execute the query.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query type is invalid or not supported.</exception>
        public async Task<AzureDevOpsActionResult<PredefinedQuery>> ListMyWorkItemsAsync(string queryType = "assignedtome", int? top = null, bool? includeCompleted = null, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                PredefinedQuery query = await _workClient.GetPredefinedQueryResultsAsync(ProjectName, queryType, top, includeCompleted, userState, cancellationToken);
                return AzureDevOpsActionResult<PredefinedQuery>.Success(query, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<PredefinedQuery>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves all work items associated with a specific team iteration (sprint), providing
        /// a comprehensive view of sprint scope and progress. This includes work items directly
        /// assigned to the iteration as well as capacity and burndown information. Essential
        /// for sprint planning, daily standups, and sprint reviews in Agile methodologies.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="iterationId">The unique identifier of the iteration whose work items to retrieve.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the iteration work items with capacity and progress information,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown when iterationId is an empty GUID.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view iteration work items.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team, project, or iteration doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<IterationWorkItems>> GetWorkItemsForIterationAsync(TeamContext teamContext, Guid iterationId, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IterationWorkItems workItems = await _workClient.GetIterationWorkItemsAsync(teamContext, iterationId, userState, cancellationToken);
                return AzureDevOpsActionResult<IterationWorkItems>.Success(workItems, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IterationWorkItems>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves all iterations configured for a team, optionally filtered by timeframe.
        /// Iterations represent the sprint or time-boxed work periods used in Agile development.
        /// This method provides access to the complete iteration schedule, enabling sprint planning,
        /// retrospectives, and long-term capacity planning across multiple iterations.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="timeFrame">Optional timeframe filter (e.g., "current", "past", "future") to limit results.</param>
        /// <param name="userState">Optional user state information for the request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of team iterations,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view team iterations.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team or project doesn't exist, or timeframe is invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> ListIterationsAsync(TeamContext teamContext, string? timeFrame = null, object? userState = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<TeamSettingsIteration> iterations = await _workClient.GetTeamIterationsAsync(teamContext, timeFrame, userState, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Success(iterations, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Creates multiple new iterations in a project based on the provided configuration options.
        /// This method enables bulk creation of sprint iterations, which is useful for setting up
        /// long-term sprint schedules or migrating from other planning tools. Each iteration is
        /// created with specified start/end dates, names, and hierarchical relationships.
        /// </summary>
        /// <param name="projectName">The name of the project where iterations will be created.</param>
        /// <param name="iterations">A collection of iteration creation options specifying the details for each new iteration.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of created work item classification nodes,
        /// or error details if the creation fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when projectName is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when iterations collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to create iterations.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the project doesn't exist or iteration data is invalid.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>> CreateIterationsAsync(string projectName, IEnumerable<IterationCreateOptions> iterations, CancellationToken cancellationToken = default)
        {
            try
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

                return AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>.Success(created, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<WorkItemClassificationNode>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Assigns multiple iterations to a team based on the provided assignment configuration options.
        /// This method enables bulk assignment of previously created iterations to specific teams,
        /// establishing which iterations the team will work within. Essential for sprint planning
        /// and organizing team work across multiple iterations or release cycles.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="iterations">A collection of iteration assignment options specifying which iterations to assign and their settings.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing a read-only list of assigned team settings iterations,
        /// or error details if the assignment fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext or iterations collection is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to assign iterations to teams.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team, project, or specified iterations don't exist.</exception>
        public async Task<AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>> AssignIterationsAsync(TeamContext teamContext, IEnumerable<IterationAssignmentOptions> iterations, CancellationToken cancellationToken = default)
        {
            try
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

                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Success(assigned, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<IReadOnlyList<TeamSettingsIteration>>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Retrieves the area path configuration and field values for a team, providing information
        /// about the organizational hierarchy and team boundaries. Area paths represent the organizational
        /// structure of work (teams, components, features) and help filter and organize work items
        /// according to team ownership and responsibility boundaries.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the team field values with area path information,
        /// or error details if the operation fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view team area configuration.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team or project doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<TeamFieldValues>> ListAreasAsync(TeamContext teamContext, CancellationToken cancellationToken = default)
        {
            try
            {
                TeamFieldValues values = await _workClient.GetTeamFieldValuesAsync(teamContext, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<TeamFieldValues>.Success(values, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<TeamFieldValues>.Failure(ex, Logger);
            }
        }

        /// <summary>
        /// Exports comprehensive information about a specific board including its configuration,
        /// columns, rules, and associated work items. This provides a complete snapshot of the board
        /// state, useful for reporting, analysis, backup purposes, or migrating board configurations.
        /// The export includes all metadata necessary to understand the board's structure and current state.
        /// </summary>
        /// <param name="teamContext">The team context containing project and team information.</param>
        /// <param name="boardId">The identifier of the board to export.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="AzureDevOpsActionResult{T}"/> containing the complete board information,
        /// or error details if the export fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when teamContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown when boardId is null or empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the API request fails or returns an error status.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to export board information.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the team, project, or board doesn't exist.</exception>
        public async Task<AzureDevOpsActionResult<Board>> ExportBoardAsync(TeamContext teamContext, string boardId, CancellationToken cancellationToken = default)
        {
            try
            {
                Board board = await _workClient.GetBoardAsync(teamContext, boardId, cancellationToken: cancellationToken);
                return AzureDevOpsActionResult<Board>.Success(board, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<Board>.Failure(ex, Logger);
            }
        }
    }
}
