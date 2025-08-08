using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<GitAnnotatedTag>> CreateTagAsync(TagCreateOptions tagCreateOptions)
        {
            try
            {
                var annotatedTag = new GitAnnotatedTag
                {
                    Name = tagCreateOptions.Name,
                    TaggedObject = new GitObject
                    {
                        ObjectId = tagCreateOptions.CommitSha,
                        ObjectType = GitObjectType.Commit
                    },
                    TaggedBy = new GitUserDate
                    {
                        Name = tagCreateOptions.TaggerName,
                        Email = tagCreateOptions.TaggerEmail,
                        Date = tagCreateOptions.Date.UtcDateTime
                    },
                    Message = tagCreateOptions.Message
                };

                GitAnnotatedTag gitAnnotatedTag = await _gitHttpClient.CreateAnnotatedTagAsync(
                    tagObject: annotatedTag,
                    project: _projectName,
                    repositoryId: tagCreateOptions.Repository
                );

                return AzureDevOpsActionResult<GitAnnotatedTag>.Success(gitAnnotatedTag, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitAnnotatedTag>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitAnnotatedTag>> GetTagAsync(
            string repositoryId, string objectId)
        {
            try
            {
                GitAnnotatedTag result = await _gitHttpClient.GetAnnotatedTagAsync(
                    project: _projectName,
                    repositoryId: repositoryId,
                    objectId: objectId);

                return AzureDevOpsActionResult<GitAnnotatedTag>.Success(result, _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitAnnotatedTag>.Failure(ex, _logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRefUpdateResult>> DeleteTagAsync(string repositoryId, string tagName)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: _projectName,
                    filter: "tags/");

                GitRef? tagRef = refs.FirstOrDefault(r => r.Name.EndsWith($"/{tagName}"));

                if(tagRef == null)
                    return AzureDevOpsActionResult<GitRefUpdateResult>.Failure($"Tag '{tagName}' not found.", _logger);

                var refUpdate = new GitRefUpdate
                {
                    Name = tagRef.Name,
                    OldObjectId = tagRef.ObjectId,
                    NewObjectId = "0000000000000000000000000000000000000000"
                };

                List<GitRefUpdateResult> gitRefUpdateResultList = await _gitHttpClient.UpdateRefsAsync(
                    refUpdates: new[] { refUpdate },
                    repositoryId: repositoryId,
                    project: _projectName);

                if(gitRefUpdateResultList.Count == 0)
                    return AzureDevOpsActionResult<GitRefUpdateResult>.Failure("Failed to delete tag.", _logger);

                return AzureDevOpsActionResult<GitRefUpdateResult>.Success(gitRefUpdateResultList[0], _logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRefUpdateResult>.Failure(ex, _logger);
            }
        }
    }
}
