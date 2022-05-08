namespace Zytonomy.Api.Model.Visitors;

/// <summary>
/// Interface that defines a contract for classes that support extraction of images
/// from base64 encoded content strings.  The extracted images are stored in Azure Storage
/// and the content is replaced with the URL of the Azure Storage content.
/// </summary>
public interface IExtractable
{
    /// <summary>
    /// The container name that the extracted binary content will be stored in.
    /// </summary>
    string ExtractContainerName { get; }

    /// <summary>
    /// The base path within the container for the image file.
    /// </summary>
    string ExtractContainerPath { get; }

    /// <summary>
    /// Returns the original content which includes embedded base64 strings.
    /// </summary>
    string GetOriginalContent();

    /// <summary>
    /// Replaces the original content with the content post extraction which has the base64 content replaced with URLs.
    /// </summary>
    /// <param name="content">The content with base64 strings replaced with URLs.</param>
    void SetExtractedContent(string content);

    /// <summary>
    /// Prepares the URL if necessary.
    /// </summary>
    /// <param name="url">The actual URI of the uploaded file.</param>
    /// <returns>An updated URL</returns>
    string PrepareUrl(string url);
}
