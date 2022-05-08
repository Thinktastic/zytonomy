namespace Zytonomy.Api.Model.Embedded;

/// <summary>
/// A content source models the handle to a specific input source of content such
/// as a PDF.
/// </summary>
public class ContentSource
{
    /// <summary>
    /// The UTC date and time when the content source was added.
    /// </summary>
    public string AddedUtc;

    /// <summary>
    /// The user that added the content source.
    /// </summary>
    public string AddedById;

    /// <summary>
    /// The display name for the content source; if none specified, then the original file name.
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// The original file name including the extension of the file.
    /// </summary>
    public string OriginalFileName;

    /// <summary>
    /// The file name in BLOB storage
    /// </summary>
    public string BlobStorageFileName;

    /// <summary>
    /// The status of the content source.
    /// </summary>
    public string Status;

    /// <summary>
    /// Retrieves the file name part only.
    /// </summary>
    /// <returns>The file name part without the path.</returns>
    public string GetBlobStorageFileNameOnly() {
        string[] parts = BlobStorageFileName.Split('/');

        return parts[parts.Length - 1];
    }
}
