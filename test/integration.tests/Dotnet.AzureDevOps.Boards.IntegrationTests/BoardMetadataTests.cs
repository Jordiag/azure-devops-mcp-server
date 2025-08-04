using System.Text.Json;
using Dotnet.AzureDevOps.Core.Boards;
using Dotnet.AzureDevOps.Core.Boards.Options;
using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.ProjectSettings;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Dotnet.AzureDevOps.Tests.Common.Attributes;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace Dotnet.AzureDevOps.Boards.IntegrationTests;

[TestType(TestType.Integration)]
[Component(Component.Boards)]
public class BoardMetadataTests : BoardsTestBase
{
    public BoardMetadataTests(IntegrationTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ReadEpic_SucceedsAsync()
    {
        var createOptions = new WorkItemCreateOptions
        {
            Title = "Read Epic Test",
            Description = "This epic is for read test",
            Tags = "IntegrationTest;Read",
        };

        int epicId = await _workItemHelper.CreateEpicAsync(createOptions);

        WorkItem? epicWorkItem = await _workItemsClient.GetWorkItemAsync(epicId);
        Assert.NotNull(epicWorkItem);
        Assert.True(epicWorkItem.Fields.ContainsKey("System.Title"));
        Assert.Equal("Read Epic Test", epicWorkItem.Fields["System.Title"]);
        Assert.True(epicWorkItem.Fields.ContainsKey("System.Description"));
        Assert.Equal("This epic is for read test", epicWorkItem.Fields["System.Description"]);
        Assert.True(epicWorkItem.Fields.ContainsKey("System.Tags"));
        string? actualTags = epicWorkItem.Fields["System.Tags"].ToString();
        Assert.Contains("IntegrationTest", actualTags);
        Assert.Contains("Read", actualTags);
    }

    [Fact]
    public async Task QueryWorkItems_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Query Epic",
            Tags = "IntegrationTest;Query",
        });

        string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.Tags] CONTAINS 'Query'";
        IReadOnlyList<WorkItem> list = await _workItemsClient.QueryWorkItemsAsync(wiql);
        Assert.Contains(list, w => w.Id == epicId);
    }

    [Fact]
    public async Task AddAndReadComments_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Comment Epic",
            Tags = "IntegrationTest;Comment",
        });

        const string commentText = "Integration comment";
        await _workItemsClient.AddCommentAsync(epicId, _azureDevOpsConfiguration.ProjectName, commentText);

        IReadOnlyList<WorkItemComment> comments = await _workItemsClient.GetCommentsAsync(epicId);
        Assert.Contains(comments, c => c.Text == commentText);
    }

    [Fact]
    public async Task AttachAndDownload_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "Attachment Epic",
            Tags = "IntegrationTest;Attach",
        });

        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "attachment");

        Guid? attachmentId = await _workItemsClient.AddAttachmentAsync(epicId, tempFile);
        Assert.NotNull(attachmentId);

        using Stream? stream = await _workItemsClient.GetAttachmentAsync(_azureDevOpsConfiguration.ProjectName, attachmentId.Value);
        Assert.NotNull(stream);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        Assert.True(memoryStream.Length > 0);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task WorkItemHistory_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync(new WorkItemCreateOptions
        {
            Title = "History Epic",
            Description = "Initial",
            Tags = "IntegrationTest;History",
        });

        await _workItemsClient.UpdateEpicAsync(epicId, new WorkItemCreateOptions { Description = "Updated" });

        IReadOnlyList<WorkItemUpdate> history = await _workItemsClient.GetHistoryAsync(epicId);
        Assert.True(history.Count > 1);
    }

    [Fact]
    public async Task LinkManagement_SucceedsAsync()
    {
        int first = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Link A", Tags = "IntegrationTest;Link" });
        int second = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Link B", Tags = "IntegrationTest;Link" });

        await _workItemsClient.AddLinkAsync(first, second, "System.LinkTypes.Related");

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(first);
        Assert.Contains(links, l => l.Url.Contains(second.ToString()));

        string linkUrl = links.First(l => l.Url.Contains(second.ToString())).Url!;
        await _workItemsClient.RemoveLinkAsync(first, linkUrl);

        IReadOnlyList<WorkItemRelation> after = await _workItemsClient.GetLinksAsync(first);
        Assert.DoesNotContain(after, l => l.Url == linkUrl);
    }

    [Fact]
    public async Task BulkEdit_SucceedsAsync()
    {
        int a = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Bulk 1", Tags = "IntegrationTest;Bulk" });
        int b = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Bulk 2", Tags = "IntegrationTest;Bulk" });

        var updates = new (int, WorkItemCreateOptions)[]
        {
            (a, new WorkItemCreateOptions { State = "Closed" }),
            (b, new WorkItemCreateOptions { State = "Closed" })
        };

        await _workItemsClient.BulkUpdateWorkItemsAsync(updates);

        WorkItem? first = await _workItemsClient.GetWorkItemAsync(a);
        WorkItem? second = await _workItemsClient.GetWorkItemAsync(b);
        Assert.Equal("Closed", first?.Fields?["System.State"].ToString());
        Assert.Equal("Closed", second?.Fields?["System.State"].ToString());
    }

    [Fact]
    public async Task ListBoards_SucceedsAsync()
    {
        TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
        List<BoardReference> boardReferences = await _workItemsClient.ListBoardsAsync(teamContext);
        Assert.NotEmpty(boardReferences);
    }

    [Fact]
    public async Task GetTeamIteration_SucceedsAsync()
    {
        TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
        List<TeamSettingsIteration> iterations = await _workItemsClient.GetTeamIterationsAsync(teamContext, string.Empty);
        Assert.NotEmpty(iterations);

        TeamSettingsIteration iteration = iterations.First();
        TeamSettingsIteration fetched = await _workItemsClient.GetTeamIterationAsync(teamContext, iteration.Id);
        Assert.Equal(iteration.Id, fetched.Id);
    }

    [Fact]
    public async Task AddHyperlink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();

        string url = "https://example.com";
        await _workItemsClient.AddHyperlinkAsync(epicId, url, "link");

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url == url);
    }

    [Fact]
    public async Task ReplaceHyperlink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        string url = "https://example.com";
        await _workItemsClient.AddHyperlinkAsync(epicId, url, "link");

        await _workItemsClient.ReplaceHyperlinkAsync(epicId, url, "https://replacement.com");

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url == "https://replacement.com");
    }

    [Fact]
    public async Task ReplaceMultipleHyperlinks_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        string url = "https://example.com";
        await _workItemsClient.AddHyperlinkAsync(epicId, url, "link");

        var replacements = new List<(string, string)> { (url, "https://replacement.com") };
        await _workItemsClient.ReplaceHyperlinksAsync(epicId, replacements);

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url == "https://replacement.com");
    }

    [Fact]
    public async Task HyperLinkExists_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        string url = "https://example.com";
        await _workItemsClient.AddHyperlinkAsync(epicId, url, "link");

        bool exists = await _workItemsClient.HyperlinkExistsAsync(epicId, url);
        Assert.True(exists);
    }

    [Fact]
    public async Task RemoveHyperlink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        string url = "https://example.com";
        await _workItemsClient.AddHyperlinkAsync(epicId, url, "link");

        await _workItemsClient.RemoveHyperlinkAsync(epicId, url);
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.DoesNotContain(links, l => l.Url == url);
    }

    [Fact]
    public async Task AddRepositoryLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();

        GitRepository repo = await _reposClient.GetRepositoryAsync(_repositoryName);
        await _workItemsClient.AddRepositoryLinkAsync(epicId, repo.Id.ToString(), "repo");

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url.Contains(repo.Id.ToString()));
    }

    [Fact]
    public async Task ReplaceRepositoryLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();

        GitRepository repo = await _reposClient.GetRepositoryAsync(_repositoryName);
        await _workItemsClient.AddRepositoryLinkAsync(epicId, repo.Id.ToString(), "repo");

        await _workItemsClient.ReplaceRepositoryLinkAsync(epicId, repo.Id.ToString(), repo.Id.ToString());
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url.Contains(repo.Id.ToString()));
    }

    [Fact]
    public async Task RemoveRepositoryLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitRepository repo = await _reposClient.GetRepositoryAsync(_repositoryName);
        await _workItemsClient.AddRepositoryLinkAsync(epicId, repo.Id.ToString(), "repo");

        await _workItemsClient.RemoveRepositoryLinkAsync(epicId, repo.Id.ToString());
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.DoesNotContain(links, l => l.Url.Contains(repo.Id.ToString()));
    }

    [Fact]
    public async Task AddPullRequestLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitPullRequest pr = await CreatePullRequestAsync();
        await _workItemsClient.AddPullRequestLinkAsync(epicId, pr.PullRequestId); 

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url.Contains(pr.PullRequestId.ToString()));
    }

    [Fact]
    public async Task ReplacePullRequestLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitPullRequest pr = await CreatePullRequestAsync();
        await _workItemsClient.AddPullRequestLinkAsync(epicId, pr.PullRequestId);
        await _workItemsClient.ReplacePullRequestLinkAsync(epicId, pr.PullRequestId, pr.PullRequestId);
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url.Contains(pr.PullRequestId.ToString()));
    }

    [Fact]
    public async Task RemovePullRequestLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitPullRequest pr = await CreatePullRequestAsync();
        await _workItemsClient.AddPullRequestLinkAsync(epicId, pr.PullRequestId);

        await _workItemsClient.RemovePullRequestLinkAsync(epicId, pr.PullRequestId);
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.DoesNotContain(links, l => l.Url.Contains(pr.PullRequestId.ToString()));
    }

    [Fact]
    public async Task AddCommitLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitCommitRef commit = await CreateCommitAsync();
        await _workItemsClient.AddCommitLinkAsync(epicId, commit.Repository.Id.ToString(), commit.CommitId, "commit");

        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url.Contains(commit.CommitId));
    }

    [Fact]
    public async Task ReplaceCommitLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitCommitRef commit = await CreateCommitAsync();
        await _workItemsClient.AddCommitLinkAsync(epicId, commit.Repository.Id.ToString(), commit.CommitId, "commit");

        await _workItemsClient.ReplaceCommitLinkAsync(epicId, commit.Repository.Id.ToString(), commit.CommitId, commit.CommitId);
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.Contains(links, l => l.Url.Contains(commit.CommitId));
    }

    [Fact]
    public async Task RemoveCommitLink_SucceedsAsync()
    {
        int epicId = await _workItemHelper.CreateEpicAsync();
        GitCommitRef commit = await CreateCommitAsync();
        await _workItemsClient.AddCommitLinkAsync(epicId, commit.Repository.Id.ToString(), commit.CommitId, "commit");

        await _workItemsClient.RemoveCommitLinkAsync(epicId, commit.Repository.Id.ToString(), commit.CommitId);
        IReadOnlyList<WorkItemRelation> links = await _workItemsClient.GetLinksAsync(epicId);
        Assert.DoesNotContain(links, l => l.Url.Contains(commit.CommitId));
    }

    [Fact]
    public async Task CreateIterations_SucceedsAsync()
    {
        string name = $"it-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var iterations = new List<IterationCreateOptions>
        {
            new IterationCreateOptions { IterationName = name }
        };

        IReadOnlyList<WorkItemClassificationNode> created = await _workItemsClient.CreateIterationsAsync(_azureDevOpsConfiguration.ProjectName, iterations);
        Assert.NotEmpty(created);
    }

    [Fact]
    public async Task AssignIterations_SucceedsAsync()
    {
        TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
        List<TeamSettingsIteration> existing = await _workItemsClient.ListIterationsAsync(teamContext);
        Assert.NotEmpty(existing);

        TeamSettingsIteration iteration = existing.First();
        var assignments = new List<IterationAssignmentOptions>
        {
            new IterationAssignmentOptions { Identifier = iteration.Id, Path = iteration.Path! }
        };

        IReadOnlyList<TeamSettingsIteration> result = await _workItemsClient.AssignIterationsAsync(teamContext, assignments);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ListAreas_SucceedsAsync()
    {
        TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
        TeamFieldValues areas = await _workItemsClient.ListAreasAsync(teamContext);
        Assert.NotEmpty(areas.Values);
    }

    [Fact]
    public async Task ExportBoard_SucceedsAsync()
    {
        TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
        List<BoardReference> boards = await _workItemsClient.ListBoardsAsync(teamContext);
        Assert.NotEmpty(boards);

        Board? board = await _workItemsClient.ExportBoardAsync(teamContext, boards.First().Id.ToString());
        Assert.NotNull(board);
    }

    [Fact]
    public async Task GetWorkItemCount_SucceedsAsync()
    {
        string wiql = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project";
        int count = await _workItemsClient.GetWorkItemCountAsync(wiql);
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task ExecuteBatch_SucceedsAsync()
    {
        int id = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Batch Root" });

        var request = new WitBatchRequest
        {
            Method = "GET",
            Uri = $"/_apis/wit/workitems/{id}?api-version={GlobalConstants.ApiVersion}"
        };

        IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.ExecuteBatchAsync(new[] { request });
        Assert.NotEmpty(responses);
    }

    [Fact]
    public async Task CreateWorkItemsBatch_SucceedsAsync()
    {
        var items = new List<WorkItemCreateOptions>
        {
            new() { Title = "Batch Item 1" },
            new() { Title = "Batch Item 2" }
        };

        IReadOnlyList<int> created = await _workItemsClient.CreateWorkItemsMultipleCallsAsync("Task", items, CancellationToken.None);
        Assert.Equal(2, created.Count);
        foreach (int workItem in created)
        {
            _createdWorkItemIds.Add(workItem);
        }
    }

    [Fact]
    public async Task UpdateWorkItemsBatch_SucceedsAsync()
    {
        int firstId = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Batch Update 1" });
        int secondId = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Batch Update 2" });

        var updates = new List<(int, WorkItemCreateOptions)>
        {
            (firstId, new WorkItemCreateOptions { State = "Closed" }),
            (secondId, new WorkItemCreateOptions { State = "Closed" })
        };

        IReadOnlyList<WitBatchResponse> batch = await _workItemsClient.UpdateWorkItemsBatchAsync(updates);
        Assert.Equal(2, batch.Count);
    }

    [Fact]
    public async Task LinkWorkItemsBatch_SucceedsAsync()
    {
        int parentId = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Batch Parent" });
        int childId = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Batch Child" });

        var links = new List<(int, int, string)> { (parentId, childId, "System.LinkTypes.Related") };

        IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.LinkWorkItemsBatchAsync(links);
        Assert.NotEmpty(responses);
    }

    [Fact]
    public async Task CloseWorkItemsBatch_SucceedsAsync()
    {
        int id1 = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Close 1" });
        int id2 = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Close 2" });

        IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.CloseWorkItemsBatchAsync(new[] { id1, id2 });
        Assert.Equal(2, responses.Count);
    }

    [Fact]
    public async Task CloseAndLinkDuplicatesBatch_SucceedsAsync()
    {
        int canonical = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Canonical" });
        int duplicate = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Duplicate" });

        var pairs = new List<(int, int)> { (duplicate, canonical) };

        IReadOnlyList<WitBatchResponse> responses = await _workItemsClient.CloseAndLinkDuplicatesBatchAsync(pairs);
        Assert.Single(responses);
    }

    [Fact]
    public async Task GetWorkItemsBatchByIds_SucceedsAsync()
    {
        int item1 = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "BatchGet 1" });
        int item2 = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "BatchGet 2" });

        IReadOnlyList<WorkItem> items = await _workItemsClient.GetWorkItemsBatchByIdsAsync(new[] { item1, item2 });
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task CreateWorkItem_SucceedsAsync()
    {
        var fields = new List<WorkItemFieldValue>
        {
            new WorkItemFieldValue { Name = "System.Title", Value = "Arbitrary" }
        };

        WorkItem? workItem = await _workItemsClient.CreateWorkItemAsync("Task", fields);
        Assert.NotNull(workItem);
        if (workItem.Id != null)
        {
            _createdWorkItemIds.Add(workItem.Id.Value);
        }
    }

    [Fact]
    public async Task UpdateWorkItem_SucceedsAsync()
    {
        int itemId = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "Update Arbitrary" });

        var updates = new List<Core.Boards.Options.WorkItemFieldUpdate>
        {
            new() { Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add, Path = "/fields/System.Title", Value = "Updated" }
        };

        WorkItem? updated = await _workItemsClient.UpdateWorkItemAsync(itemId, updates);
        Assert.NotNull(updated);
    }

    [Fact]
    public async Task GetWorkItemType_SucceedsAsync()
    {
        WorkItemType type = await _workItemsClient.GetWorkItemTypeAsync(_azureDevOpsConfiguration.ProjectName, "Task");
        Assert.NotNull(type);
    }

    [Fact]
    public async Task GetQuery_SucceedsAsync()
    {
        QueryHierarchyItem query = await _workItemsClient.GetQueryAsync(_azureDevOpsConfiguration.ProjectName, "Shared Queries");
        Assert.NotNull(query);
    }

    [Fact]
    public async Task GetQueryResultsById_SucceedsAsync()
    {
        string queryName = "My Test Query";
        string wiql = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.WorkItemType] = 'Task'";
        await _workItemsClient.CreateSharedQueryAsync(_azureDevOpsConfiguration.ProjectName, queryName, wiql);

        QueryHierarchyItem root = await _workItemsClient.GetQueryAsync(
            _azureDevOpsConfiguration.ProjectName,
            "Shared Queries",
            depth: 2);

        QueryHierarchyItem? queryItem = FindFirstQuery(root);
        Assert.NotNull(queryItem);

        TeamContext teamContext = new TeamContext(_azureDevOpsConfiguration.ProjectName);
        WorkItemQueryResult result = await _workItemsClient.GetQueryResultsByIdAsync(queryItem!.Id, teamContext);
        Assert.NotNull(result);

        await _workItemsClient.DeleteSharedQueryAsync(_azureDevOpsConfiguration.ProjectName, queryName);
    }

    private QueryHierarchyItem? FindFirstQuery(QueryHierarchyItem root)
    {
        var stack = new Stack<QueryHierarchyItem>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            QueryHierarchyItem current = stack.Pop();
            if (current.IsFolder == false || current.IsFolder == null)
            {
                return current;
            }
            if (current.Children != null)
            {
                for (int i = current.Children.Count - 1; i >= 0; i--)
                {
                    stack.Push(current.Children[i]);
                }
            }
        }
        return null;
    }

    [Fact]
    public async Task LinkWorkItemsByNameBatch_SucceedsAsync()
    {
        int w1 = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "LinkName1" });
        int w2 = await _workItemHelper.CreateTaskAsync(options: new WorkItemCreateOptions { Title = "LinkName2" });

        var links = new List<(int, int, string, string?)>
        {
            (w1, w2, "related", "link")
        };

        IReadOnlyList<WitBatchResponse> resp = await _workItemsClient.LinkWorkItemsByNameBatchAsync(links);
        Assert.Single(resp);
    }

    private async Task<GitPullRequest> CreatePullRequestAsync()
    {
        var create = new GitPullRequestCreateOptions
        {
            SourceRefName = _sourceBranch,
            TargetRefName = _targetBranch,
            Title = "PR from tests",
        };

        GitPullRequest pr = await _reposClient.CreatePullRequestAsync(create, _repositoryName);
        _createdPullRequestIds.Add(pr.PullRequestId);
        return pr;
    }

    private async Task<GitCommitRef> CreateCommitAsync()
    {
        string filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "test");

        GitCommitRef commit = await _reposClient.CreateCommitAsync(_repositoryName, _sourceBranch, _targetBranch, new [] { filePath });
        File.Delete(filePath);
        return commit;
    }
}
