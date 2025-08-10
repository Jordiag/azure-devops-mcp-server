using Dotnet.AzureDevOps.Core.Common;
using Dotnet.AzureDevOps.Core.Repos.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Dotnet.AzureDevOps.Core.Repos
{
    public partial class ReposClient
    {
        public async Task<AzureDevOpsActionResult<GitAnnotatedTag>> CreateTagAsync(TagCreateOptions tagCreateOptions, CancellationToken cancellationToken = default)
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
                    project: ProjectName,
                    repositoryId: tagCreateOptions.Repository,
                    cancellationToken: cancellationToken
                );

                return AzureDevOpsActionResult<GitAnnotatedTag>.Success(gitAnnotatedTag, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitAnnotatedTag>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitAnnotatedTag>> GetTagAsync(
            string repositoryId, string objectId, CancellationToken cancellationToken = default)
        {
            try
            {
                GitAnnotatedTag result = await _gitHttpClient.GetAnnotatedTagAsync(
                    project: ProjectName,
                    repositoryId: repositoryId,
                    objectId: objectId,
                    cancellationToken: cancellationToken);

                return AzureDevOpsActionResult<GitAnnotatedTag>.Success(result, Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitAnnotatedTag>.Failure(ex, Logger);
            }
        }

        public async Task<AzureDevOpsActionResult<GitRefUpdateResult>> DeleteTagAsync(string repositoryId, string tagName, CancellationToken cancellationToken = default)
        {
            try
            {
                List<GitRef> refs = await _gitHttpClient.GetRefsAsync(
                    repositoryId: repositoryId,
                    project: ProjectName,
                    filter: "tags/",
                    cancellationToken: cancellationToken);

                GitRef? tagRef = refs.FirstOrDefault(r => r.Name.EndsWith($"/{tagName}"));

                if(tagRef == null)
                    return AzureDevOpsActionResult<GitRefUpdateResult>.Failure($"Tag '{tagName}' not found.", Logger);

                var refUpdate = new GitRefUpdate
                {
                    Name = tagRef.Name,
                    OldObjectId = tagRef.ObjectId,
                    NewObjectId = "0000000000000000000000000000000000000000"
                };

                List<GitRefUpdateResult> gitRefUpdateResultList = await _gitHttpClient.UpdateRefsAsync(
                    refUpdates: new[] { refUpdate },
                    repositoryId: repositoryId,
                    project: ProjectName,
                    cancellationToken: cancellationToken);

                if(gitRefUpdateResultList.Count == 0)
                    return AzureDevOpsActionResult<GitRefUpdateResult>.Failure("Failed to delete tag.", Logger);

                return AzureDevOpsActionResult<GitRefUpdateResult>.Success(gitRefUpdateResultList[0], Logger);
            }
            catch(Exception ex)
            {
                return AzureDevOpsActionResult<GitRefUpdateResult>.Failure(ex, Logger);
            }
        }
    }
}

