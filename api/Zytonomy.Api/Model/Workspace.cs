using Member = Zytonomy.Api.Model.Embedded.Member;

namespace Zytonomy.Api.Model;

/// <summary>
/// Abstraction for a content set mapped to a knowledge base.  A knowledge base is the
/// Cognitive Services container for the content items.  A content set is an abstraction
/// of a set of content sources mapped to a knowledge base.
/// </summary>
[Container("Core")]
public class Workspace : DocumentEntityBase
{
    /// <summary>
    /// The ID of the QA knowledge base.
    /// </summary>
    public string KbId;

    /// <summary>
    /// The list of content sources associated with this content set.
    /// </summary>
    public List<ContentSource> Sources;

    /// <summary>
    /// Get or set the description associated with this content set.
    /// </summary>
    public string Description;

    /// <inheritdoc/>
    public override string PartitionKey => CreatedBy.Id;

    /// <summary>
    /// The ref of the user who created the workspace.
    /// </summary>
    public GenericRef CreatedBy;

    /// <summary>
    /// The status of the workspace.
    /// </summary>
    public string Status;

    /// <summary>
    /// The list of user members and roles for this workspace.
    /// </summary>
    public List<Member> Members;

    /// <summary>
    /// Adds files to a blob storage endpoint
    /// </summary>
    /// <param name="blobBinder">The binder used to access the blob storage functionality.</param>
    /// <param name="files">A collection of file objects submitted via an API call.</param>
    public void AddFiles(string addedById, IBinder blobBinder, IFormFileCollection files)
    {
        DateTime now = DateTime.UtcNow;

        foreach(IFormFile file in files)
        {
            Guid fileId = Guid.NewGuid();

            string path = $"zytonomy/workspaces/{Id}/sources/{fileId}.pdf";

            // Files are always sent to remove storage, even during development since they need to be visible.
            using(Stream blobStream = blobBinder.Bind<Stream>(new BlobAttribute(path, FileAccess.Write){ Connection = "ContentSourceStorage" }))
            using(Stream fileStream = file.OpenReadStream()){
                fileStream.CopyTo(blobStream);
            }

            Sources.Add(new ContentSource {
                DisplayName = file.Name,
                OriginalFileName = file.FileName,
                BlobStorageFileName = path,
                AddedById = addedById,
                AddedUtc = now.ToString("u"),
                Status = "Publishing"
            });
        }
    }
}
