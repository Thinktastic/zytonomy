namespace Zytonomy.Api.Model.Visitors;

/// <summary>
/// Visitor pattern for extracting images from entities which
/// can contain binary base64 content.
/// </summary>
public class ImageExtractor
{
    private static MD5 MD5 = MD5.Create();
    private static Uri WEB_URI = new Uri(Environment.GetEnvironmentVariable("ContentSourceStorage_WebUri"));
    private static readonly FileExtensionContentTypeProvider MIME = new FileExtensionContentTypeProvider();

    public void Extract(IExtractable source)
    {
        BlobContainerClient blobs = BlobContainerFactory.Create(source.ExtractContainerName);

        // Process the body <img src="data:image/png;base64,iVBORw0KGgoAAA...
        string content = Regex.Replace(source.GetOriginalContent(),
            "src=\"data:(?'contenttype'[^;]+);base64,(?'base64'[^\"]+)\"",
            (Match match) =>
            {
                string base64 = match.Groups["base64"].Value;

                byte[] bytes = Convert.FromBase64String(base64);

                BinaryData data = new BinaryData(bytes);

                string signature = Regex.Replace(
                    Convert.ToBase64String(MD5.ComputeHash(bytes)).ToLowerInvariant(),
                    @"[^a-z0-9\-]",
                    string.Empty);

                string extension = CalculateExtension(base64[0]);

                string filename = $"{signature}.{extension}";

                string path = $"${source.ExtractContainerPath.TrimEnd('/')}/{filename}";

                // Upload the image to Azure Storage
                BlobClient client = blobs.GetBlobClient(path);

                if (!MIME.TryGetContentType(filename, out string contentType))
                {
                    contentType = "application/octet-stream";
                }

                if(!client.Exists())
                {
                    using(Stream stream = data.ToStream())
                    {
                        client.Upload(stream, new BlobHttpHeaders
                        {
                            ContentType = contentType
                        });
                    }
                }

                Uri uri = client.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddYears(20));

                string url = uri.AbsoluteUri;

                url = source.PrepareUrl(url);

                return $"src=\"{url}\"";
            });

        source.SetExtractedContent(content);
    }

    /// <summary>
    /// Images may not come with actual extension so instead, use the first character of the base64 string.
    /// See: https://stackoverflow.com/a/50111377/116051
    /// </summary>
    private string CalculateExtension(char hint) =>
        hint switch {
            '/' => "jpg",
            'i' => "png",
            'R' => "gif",
            'U' => "webp",
            _ => "jpg"
        };
}
