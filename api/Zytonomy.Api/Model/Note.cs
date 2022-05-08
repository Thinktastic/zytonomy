namespace Zytonomy.Api.Model;

/// <summary>
/// The note class represents a user saved note.  Notes can be saved from a KB source
/// or saved as a direct entry.  Notes that are direct entry can be sumbmitted to the
/// KB to answer other people's questions.
/// </summary>
[Container("Core")]
public class Note : DocumentEntityBase, IExtractable
{
    /// <summary>
    /// The ID of the workspace that the note is associated with.
    /// </summary>
    public string WorkspaceId;

    /// <summary>
    /// The user that created the note.
    /// </summary>
    public GenericRef Author;

    /// <summary>
    /// The body of the note.
    /// </summary>
    public string Body;

    /// <summary>
    /// An icon associated with the note in the UI.
    /// </summary>
    public string Icon;

    /// <summary>
    /// The color to associate with the note in the UI.
    /// </summary>
    public string Color;

    /// <summary>
    /// The ID of the KB that the entry was retrieved from; can be empty.
    /// </summary>
    public string KbId;

    /// <summary>
    /// The ID of the entry in the KB that the note was copied from; can be empty.
    /// </summary>
    public int KbEntryId;

    /// <summary>
    /// The content source for this note.
    /// </summary>
    public ContentSource Source;

    /// <summary>
    /// A list of tags that identify this note.
    /// </summary>
    public List<string> Tags;

    /// <summary>
    /// An importantance flag associated with this note.
    /// </summary>
    public string Importance;

    /// <summary>
    /// A Boolean flag indicating whether the note is private for the author or whether it's shared with other users
    /// in the workspace.
    /// </summary>
    public bool IsPrivate;

    /// <summary>
    /// The UTC date and time that the note was created.
    /// </summary>
    public string CreatedUtc;

    /// <summary>
    /// The list of comments that users have entered against the note.
    /// </summary>
    public List<Comment> Comments;

    /// <summary>
    /// When the note body contains an image, the first image URL is set during upload and is used as a preview image.
    /// </summary>
    public string FirstImageUrl;

    /// <summary>
    /// When a note body contains a video, the first video URL is set during upload and can be rendered into the card.
    /// </summary>
    public string FirstVideoUrl;

    public override string PartitionKey => WorkspaceId;
    public string ExtractContainerName => "zytonomy";
    public string ExtractContainerPath => $"workspaces/{WorkspaceId}/images";

    // TODO: What happens when a user replaces an image in a note?  Need a mechanism to track and delete images on the server.
    /// <summary>
    /// Extracts images embedded in the note and replaces it with a URL.  The images are uploaded
    /// as Base64 strings and need to be extracted and uploaded to Azure Storage.
    /// </summary>
    public void ExtractImages()
    {
        ImageExtractor extractor = new ImageExtractor();

        extractor.Extract(this);
    }

    /// <summary>
    /// If the body contains a link to a video, we want to detect it and capture the first video.
    /// </summary>
    public void ExtractVideos()
    {
        Match match = Regex.Match(Body, "iframe src=\"(?'url'[^\"]+)\"");

        if(!match.Success)
        {
            return;
        }

        FirstVideoUrl = match.Groups["url"].Value;
        }

    public string GetOriginalContent()
    {
        return Body;
    }

    public string PrepareUrl(string url)
    {
        if(string.IsNullOrEmpty(FirstImageUrl))
        {
            FirstImageUrl = url;
        }

        return url; // No transform.
    }

    public void SetExtractedContent(string content)
    {
        Body = content;
    }
}
